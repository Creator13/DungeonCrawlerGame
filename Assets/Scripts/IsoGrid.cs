using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Dungen
{
    public class IsoGrid : MonoBehaviour
    {
        [SerializeField] private GeneratorSettings settings;
        [SerializeField] private Canvas labelCanvas;

        public float tileStep = 1;

        private List<Tile> tiles;

        public Vector3 StartTilePosition => GetTilePosition(StartTile);
        public Vector2Int StartTile { get; private set; }

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

            // Start tile is the center (rounded down because integer division)
            StartTile = new Vector2Int(settings.sizeX / 2, settings.sizeY / 2);
        }

        private void Start()
        {
            Generate();
        }

        public void Generate()
        {
            var size = settings.sizeX * settings.sizeY;

            tiles = new List<Tile>(settings.sizeX);

            for (var i = 0; i < size; i++)
            {
                var data = new TileData(i % settings.sizeY, i / settings.sizeX);
                var tile = TileFromData(data);
                tiles.Add(tile);
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

        public Vector3 GetTilePosition(int x, int y)
        {
            return new Vector3(x * tileStep, .5f, y * tileStep);
        }

        public Vector3 GetTilePosition(Vector2Int tile)
        {
            return GetTilePosition(tile.x, tile.y);
        }

        public Cell[,] GetCellGrid()
        {
            var cells = new Cell[settings.sizeX, settings.sizeY];

            foreach (var tile in tiles)
            {
                var cell = new Cell
                {
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
