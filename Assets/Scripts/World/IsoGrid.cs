using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Dungen.World
{
    public class IsoGrid : MonoBehaviour
    {
        [SerializeField] private GeneratorSettings settings;
        [SerializeField] private Canvas labelCanvas;

        public float tileStep = 1;

        private Tile[] tiles;

        // Start tile is the center (rounded down because integer division)
        public Vector3 StartTilePosition => GetTileWorldPosition(StartTile);
        public Vector2Int StartTile => new Vector2Int(settings.sizeX / 2, settings.sizeY / 2);

        private Cell[,] cellGrid;
        public Cell[,] CellGrid => cellGrid ??= GetCellGrid();

        private void Awake()
        {
            if (settings.useLabels)
            {
                if (labelCanvas == null)
                {
                    labelCanvas = GetComponentInChildren<Canvas>();
                }

                Assert.IsNotNull(labelCanvas);
            }
        }

        public void CreateGridFromTileDataArray(TileData[] tiles)
        {
            this.tiles = new Tile[tiles.Length];
            
            for(var i = 0; i < tiles.Length; i++)
            {
                this.tiles[i] = TileFromData(tiles[i]);
            }
        }

        private Tile TileFromData(TileData data)
        {
            var tile = Instantiate(settings.tilePrefab, transform, true);

            tile.transform.position = new Vector3(data.x, 0, data.y);
            tile.transform.localRotation = Quaternion.Euler(90, 0, 0);

            tile.Initialize(data);

            if (settings.useLabels)
            {
                var label = Instantiate(settings.tileLabel, labelCanvas.transform, false);
                label.position = tile.transform.position;
                label.GetComponent<TMP_Text>().text = $"x {data.x}\ny {data.y}";
            }

            return tile;
        }

        public Vector3 GetTileWorldPosition(int x, int y)
        {
            return new Vector3(x * tileStep, transform.position.y, y * tileStep);
        }

        public Vector3 GetTileWorldPosition(Vector2Int tile)
        {
            return GetTileWorldPosition(tile.x, tile.y);
        }

        private Cell[,] GetCellGrid()
        {
            var cells = new Cell[settings.sizeX, settings.sizeY];

            foreach (var tile in tiles)
            {
                var cell = new Cell {
                    gridPosition = new Vector2Int(tile.X, tile.Y)
                };

                if (tile.X == settings.sizeX - 1) cell.walls |= Wall.LEFT;
                if (tile.Y == settings.sizeY - 1) cell.walls |= Wall.DOWN;
                if (tile.X == 0) cell.walls |= Wall.RIGHT;
                if (tile.Y == 0) cell.walls |= Wall.UP;

                cells[tile.X, tile.Y] = cell;
            }

            return cells;
        }

        public List<Tile> GetTilesFromPositions(List<Vector2Int> positions)
        {
            var tiles = new List<Tile>(positions.Count);

            foreach (var position in positions)
            {
                var tile = this.tiles[position.x + position.y * settings.sizeY];
                Assert.AreEqual(position, new Vector2Int(tile.X, tile.Y));

                tiles.Add(tile);
            }

            return tiles;
        }
    }
}
