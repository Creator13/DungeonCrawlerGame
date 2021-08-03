using System.Collections.Generic;
using System.Linq;
using Dungen.Netcode;
using Dungen.World;
using EditorUtils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dungen.Gameplay
{
    public class SimulatedEnemy
    {
        public uint networkId;
        public uint targetId;

        public int speed;
    }

    public class GameSimulator : MonoBehaviour
    {
        [SerializeField] private GeneratorSettings settings;
        [SerializeField] private float timeBetweenTicks;
        [SerializeField] private ServerBehavior serverBehavior;

        public bool running;

        private float lastTickTime;

        public int Score { get; private set; }

        private List<SimulatedEnemy> enemies = new List<SimulatedEnemy>();

        public GeneratorSettings Settings => settings;
        public ServerGrid Grid { get; private set; }

        private DungenServer Server => serverBehavior.Server;

        private void Awake()
        {
            Grid = new ServerGrid(settings);
        }

        private void Update()
        {
            running = Server.GameStarted;
            if (!running) return;

            if (Time.time > lastTickTime + timeBetweenTicks)
            {
                DoTick();

                lastTickTime = Time.time;
            }
        }

        public Vector2Int GetRandomFreeGridPosition()
        {
            return new Vector2Int(Random.Range(0, settings.sizeX), Random.Range(0, settings.sizeY));
        }

        private void DoTick()
        {
            if (Random.value < .15f)
            {
                SpawnEnemy();
            }

            MoveEnemies();
        }

        private void SpawnEnemy()
        {
            var enemy = new SimulatedEnemy {
                networkId = DungenServer.NextNetworkId,
                speed = 1
            };

            var position = GetRandomFreeGridPosition();
            var closestPlayer = uint.MaxValue;
            var smallestDistance = int.MaxValue;
            foreach (var (playerId, playerPos) in Grid.PlayerPositions)
            {
                var distance = Astar.ManhattanDistance(position, playerPos);
                if (distance < smallestDistance)
                {
                    closestPlayer = playerId;
                    smallestDistance = distance;
                }
            }

            enemy.targetId = closestPlayer;

            enemies.Add(enemy);
            Grid.EnemyPositions[enemy.networkId] = position;

            Server.SendBroadcast(new EnemySpawnMessage {position = position, networkId = enemy.networkId});
        }

        public bool TryAttack(Vector2Int pos)
        {
            if (Grid.EnemyPositions.ContainsValue(pos))
            {
                var enemyId = Grid.EnemyPositions.First(kvp => kvp.Value == pos).Key;

                enemies.Remove(enemies.First(enemy => enemy.networkId == enemyId));
                Grid.EnemyPositions.Remove(enemyId);
                
                Server.SendBroadcast(new EnemyKilledMessage {networkId = enemyId});
                AddScore(1);
                
                return true;
            }

            return false;
        }

        public void AddScore(int toAdd)
        {
            Score += toAdd;
            Server.SendBroadcast(new ScoreUpdateMessage {newScore = Score});
        }
        
        private void MoveEnemies()
        {
            foreach (var enemy in enemies)
            {
                if (Random.value < .35) continue;

                var path = Astar.FindPathToTarget(Grid.EnemyPositions[enemy.networkId], Grid.PlayerPositions[enemy.targetId], Grid.cells);

                var nextTile = path[0];

                if (Grid.PlayerPositions.ContainsValue(nextTile))
                {
                    var player = Grid.PlayerPositions.First(kvp => kvp.Value == nextTile).Key;
                    GameOver();
                }
                else
                {
                    Grid.EnemyPositions[enemy.networkId] = nextTile;

                    Server.SendBroadcast(new EnemyMoveMessage {networkId = enemy.networkId, position = nextTile});
                }
            }
        }

        private void GameOver()
        {
            Server.EndGame();
            
            Server.SendBroadcast(new GameOverMessage {finalScore = Score});
        }
    }
}
