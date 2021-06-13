using System;
using System.Collections.Generic;
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
            Debug.Log("tick");
            if (Random.value < .2f)
            {
                SpawnEnemy();
            }
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

            Server.SendBroadcast(new EnemySpawnMessage {position = position, networkId = enemy.networkId});
        }
    }
}
