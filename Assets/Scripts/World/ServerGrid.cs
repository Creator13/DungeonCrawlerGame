using System.Collections.Generic;
using Dungen.Netcode;
using UnityEngine;
using EditorUtils;

namespace Dungen.World
{
    public class ServerGrid
    {
        private readonly GridGenerator generator;
        private readonly GeneratorSettings settings;
        private readonly Cell[,] cells;

        private TileData[] tiles;
        public Dictionary<uint, Vector2Int> PlayerPositions { get; } = new Dictionary<uint, Vector2Int>();
        public Dictionary<uint, Vector2Int> EnemyPositions { get; }= new Dictionary<uint, Vector2Int>();

        public ServerGrid(GeneratorSettings settings)
        {
            this.settings = settings;
            this.generator = new GridGenerator(settings);

            tiles = generator.GenerateGrid();
            cells = GridGenerator.GetCellGridFromTileDataGrid(tiles, settings.sizeX, settings.sizeY);
        }

        public bool SetPlayer(uint networkId, Vector2Int newPosition)
        {
            var hasPath = Astar.HasPath(PlayerPositions[networkId], newPosition, cells);
            if (hasPath)
            {
                PlayerPositions[networkId] = newPosition;
            }

            return hasPath;
        }

        public void InitializePlayer(PlayerStartData playerData)
        {
            PlayerPositions[playerData.networkId] = playerData.position;
        }
    }
}
