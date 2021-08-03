using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Dungen.Gameplay.States;
using Dungen.Highscore;
using Dungen.Netcode;
using Dungen.UI;
using Dungen.World;
using FSM;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Dungen.Gameplay
{
    public class DungenGame : MonoBehaviour
    {
        private FiniteStateMachine<DungenBlackboard> GameStateMachine { get; set; }
        private DungenBlackboard blackboard;

        [SerializeField] private NetworkedPlayerController ownPlayer;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private ClientBehaviour clientBehaviour;
        [SerializeField] private GeneratorSettings generatorSettings;
        [SerializeField] private IsoGrid grid;
        [SerializeField] private NetworkedEntityManager entityManager;
        [FormerlySerializedAs("highscoreUserManager")] [SerializeField] private PlayerHighscoreHelper playerHighscoreHelper;

        public DungenClient Client => clientBehaviour.Client;
        public PlayerHighscoreHelper PlayerHighscoreHelper => playerHighscoreHelper;

        private uint currentPlayerTurn;
        public PlayerInfo CurrentPlayerTurn => Players[currentPlayerTurn];

        public int Score { get; private set; }

        public Dictionary<uint, PlayerInfo> Players { get; } = new Dictionary<uint, PlayerInfo>();

        private void Awake()
        {
            GameStateMachine = new FiniteStateMachine<DungenBlackboard>();
            clientBehaviour = gameObject.AddComponent<ClientBehaviour>();

            blackboard = new DungenBlackboard {
                gameController = this,
                ui = uiManager,
                grid = this.grid
            };

            ownPlayer.gameObject.SetActive(false);
        }

        private void Start()
        {
            GameStateMachine.Initialize(new JoiningState(blackboard));
        }

        private void Update()
        {
            GameStateMachine.Update();
        }

        public void RequestStateChange<T>() where T : State<DungenBlackboard>
        {
            var newState = (T) Activator.CreateInstance(typeof(T), blackboard);

            GameStateMachine.ChangeState(newState);
        }

        public void ConnectToServer(string ip)
        {
            if (!playerHighscoreHelper.LoggedIn)
            {
                throw new InvalidOperationException("Can't start game without a user logged in!");
            }

            clientBehaviour.CreateAndConnect(this, playerHighscoreHelper.CurrentUser.nickname, ip);

            Client.Connected += BindClientEvents;
            Client.Disconnected += UnbindClientEvents;
        }

        public void StartServerAndConnect(string ip)
        {
            if (!playerHighscoreHelper.LoggedIn)
            {
                throw new InvalidOperationException("Can't start game without a user logged in!");
            }

            SceneManager.LoadScene("Server", LoadSceneMode.Additive);
            ConnectToServer(ip);
        }

        public void InitializeWorld(PlayerStartData[] players)
        {
            GenerateWorld();
            InstatiatePlayers(players);
        }

        public void DestroyWorld()
        {
            entityManager.UnregisterEntity(ownPlayer.NetworkId);
            ownPlayer.gameObject.SetActive(false);

            grid.gameObject.SetActive(false);
            entityManager.DespawnAll();
        }

        public void MoveEntity(uint id, Vector2Int newPosition)
        {
            entityManager.MoveEntity(id, newPosition);
        }

        public void RequestMove(Vector2Int newPosition)
        {
            var moveRequest = new MoveActionRequestMessage {newPosition = newPosition};
            Client.SendPackedMessage(moveRequest);
        }

        public void RequestAttack(Vector2Int enemyPosition)
        {
            var attackRequest = new AttackActionRequestMessage {attackPosition = enemyPosition};
            Client.SendPackedMessage(attackRequest);
        }

        public void SetTurn(uint playerId)
        {
            currentPlayerTurn = playerId;

            if (playerId == Client.OwnNetworkId)
            {
                ownPlayer.StartTurn();
            }
            else
            {
                ownPlayer.EndTurn();
            }
        }

        public void SpawnEnemy(uint id, Vector2Int position)
        {
            entityManager.SpawnEntity(entityManager.enemyPrefab, id, position);
        }

        public void RemoveEntity(uint id)
        {
            entityManager.DespawnEntity(id);
        }

        public void UpdateScore(int newScore)
        {
            Score = newScore;
        }

        private void GenerateWorld()
        {
            var generator = new GridGenerator(generatorSettings);

            grid.CreateGridFromTileDataArray(generator.GenerateGrid());
        }

        private void InstatiatePlayers(PlayerStartData[] players)
        {
            foreach (var playerStartData in players)
            {
                if (playerStartData.networkId == Client.OwnNetworkId)
                {
                    ownPlayer.gameObject.SetActive(true);
                    ownPlayer.InitializeFromNetwork(playerStartData.position);
                    entityManager.RegisterEntity(ownPlayer, Client.OwnNetworkId);
                    continue;
                }

                entityManager.SpawnEntity(entityManager.remotePlayerPrefab, playerStartData.networkId, playerStartData.position);
            }
        }

        private void BindClientEvents()
        {
            Client.PlayerJoined += OnPlayerJoined;
            Client.PlayerLeft += OnPlayerLeft;
        }

        private void UnbindClientEvents()
        {
            Client.PlayerJoined -= OnPlayerJoined;
            Client.PlayerLeft -= OnPlayerLeft;
        }

        private void OnPlayerJoined(PlayerInfo playerInfo)
        {
            Players[playerInfo.networkId] = playerInfo;
        }

        private void OnPlayerLeft(uint playerId)
        {
            Players.Remove(playerId);
            RequestStateChange<GameLeftState>();
        }
    }
}
