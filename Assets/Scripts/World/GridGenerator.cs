using System.Collections.Generic;
using UnityEngine;

namespace Dungen.World
{
    public class GridGenerator
    {
        private readonly GeneratorSettings settings;
        
        public GridGenerator(GeneratorSettings settings)
        {
            this.settings = settings;
        }
        
        public TileData[] GenerateGrid()
        {
            var size = settings.sizeX * settings.sizeY;

            var tiles = new List<TileData>(settings.sizeX * settings.sizeY);

            for (var i = 0; i < size; i++)
            {
                var data = new TileData(i % settings.sizeY, i / settings.sizeX);
                tiles.Add(data);
            }

            return tiles.ToArray();
        }

        public static Cell[,] GetCellGridFromTileDataGrid(IEnumerable<TileData> tiles, int sizeX, int sizeY)
        {
            var cells = new Cell[sizeX, sizeY];

            foreach (var tile in tiles)
            {
                var cell = new Cell {
                    gridPosition = new Vector2Int(tile.x, tile.y)
                };

                if (tile.x == sizeX - 1) cell.walls |= Wall.LEFT;
                if (tile.y == sizeY - 1) cell.walls |= Wall.DOWN;
                if (tile.x == 0) cell.walls |= Wall.RIGHT;
                if (tile.y == 0) cell.walls |= Wall.UP;

                cells[tile.x, tile.y] = cell;
            }

            return cells;
        }
    }
}
