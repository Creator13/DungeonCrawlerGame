using System;
using System.Collections.Generic;
using Dungen.Gameplay.States;
using Dungen.Netcode;
using Dungen.UI;
using Dungen.World;
using FSM;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        public DungenClient Client => clientBehaviour.Client;

        private uint currentPlayerTurn;
        public PlayerInfo CurrentPlayerTurn => Players[currentPlayerTurn];

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

        public void ConnectToServer(string name, string ip)
        {
            clientBehaviour.CreateAndConnect(this, name, ip);

            Client.Connected += BindClientEvents;
            Client.Disconnected += UnbindClientEvents;
        }

        public void StartServerAndConnect(string name, string ip)
        {
            SceneManager.LoadScene("Server", LoadSceneMode.Additive);
            ConnectToServer(name, ip);
        }

        public void InitializeWorld(PlayerStartData[] players)
        {
            GenerateWorld();
            InstatiatePlayers(players);
        }

        public void MovePlayer(uint id, Vector2Int newPosition)
        {
            entityManager.MoveEntity(id, newPosition);
        }

        public void RequestMove(Vector2Int newPosition)
        {
            var moveRequest = new MoveActionRequestMessage {newPosition = newPosition};
            Client.SendPackedMessage(moveRequest);
        }

        public void SetTurn(uint playerId)
        {
            currentPlayerTurn = playerId;
            
            if (playerId == Client.OwnNetworkId)
            {
                StartTurn();
            }
            else
            {
                EndTurn();
            }
        }

        public void StartTurn()
        {   
            ownPlayer.StartTurn();
        }

        public void EndTurn()
        {
            ownPlayer.EndTurn();
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
                    ownPlayer.InitializeFromNetwork(playerStartData);
                    entityManager.RegisterEntity(ownPlayer, Client.OwnNetworkId);
                    continue;
                }

                entityManager.SpawnEntity(entityManager.remotePlayerPrefab, playerStartData);
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
        }
    }
}
