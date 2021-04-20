using UnityEngine;

namespace Dungen
{
    public class IsoGrid : MonoBehaviour
    {
        public Vector3 StartTilePosition => GetTilePosition(0 ,0);
        public float tileStep = 1;

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
