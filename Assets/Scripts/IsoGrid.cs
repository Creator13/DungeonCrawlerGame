using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Dungen
{
    public class IsoGrid : MonoBehaviour
    {
        [SerializeField] private GeneratorSettings settings;
        [SerializeField] private Canvas labelCanvas;
        
        public float tileStep = 1;
        
        private TileData[] tiles;

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
            tiles = new TileData[settings.sizeX * settings.sizeY];

            for (var i = 0; i < tiles.Length; i++)
            {
                CreateTile(i);
            }

            BuildGrid();
        }

        private void CreateTile(int index)
        {
            tiles[index] = new TileData(index % settings.sizeY, index / settings.sizeX);
        }

        private void BuildGrid()
        {
            for (var i = 0; i < tiles.Length; i++)
            {
                var data = tiles[i];
                var tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.name = $"tile {i}";

                tile.transform.SetParent(transform);
                tile.transform.position = new Vector3(data.x, 0,  data.y);
                tile.transform.localRotation = Quaternion.Euler(90, 0, 0);

                var behavior = tile.AddComponent<TempTileBehavior>();
                behavior.x = data.x;
                behavior.y = data.y;

                if (settings.useLabels)
                {
                    var label = Instantiate(settings.tileLabel, labelCanvas.transform, false);
                    label.position = tile.transform.position;
                    label.GetComponent<TMP_Text>().text = $"x {data.x}\ny {data.y}";
                }
            }
        }
        
        public Vector3 GetTilePosition(int x, int y)
        {
            return new Vector3(x * tileStep, .5f, y * tileStep);
        }
        
        public Vector3 GetTilePosition(Vector2Int tile)
        {
            return GetTilePosition(tile.x, tile.y);
        }
    }
}
