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

        private TileData[] tiles;
        private Cell[,] cells;
        private Dictionary<uint, Vector2Int> playerPositions = new Dictionary<uint, Vector2Int>();

        public ServerGrid(GeneratorSettings settings)
        {
            this.settings = settings;
            this.generator = new GridGenerator(settings);

            tiles = generator.GenerateGrid();
            cells = GridGenerator.GetCellGridFromTileDataGrid(tiles, settings.sizeX, settings.sizeY);
        }

        public bool SetPlayer(uint networkId, Vector2Int newPosition)
        {
            var hasPath = Astar.HasPath(playerPositions[networkId], newPosition, cells);
            if (hasPath)
            {
                playerPositions[networkId] = newPosition;
            }

            return hasPath;
        }

        public void InitializePlayer(PlayerStartData playerData)
        {
            playerPositions[playerData.networkId] = playerData.position;
        }
    }
}
