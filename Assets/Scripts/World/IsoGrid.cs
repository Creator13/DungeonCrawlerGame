using System.Collections.Generic;
using System.Linq;
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

        public Cell[,] CellGrid =>
            cellGrid ??= GridGenerator.GetCellGridFromTileDataGrid(tiles.Select(tile => tile.Data), settings.sizeX, settings.sizeY);

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

            for (var i = 0; i < tiles.Length; i++)
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

        public Tile GetTileFromPosition(Vector2Int position)
        {
            return tiles[position.x + position.y * settings.sizeY];
        }

        public List<Tile> GetTilesFromPositions(List<Vector2Int> positions)
        {
            var tiles = new List<Tile>(positions.Count);

            foreach (var position in positions)
            {
                var tile = GetTileFromPosition(position);

                tiles.Add(tile);
            }

            return tiles;
        }
    }
}
