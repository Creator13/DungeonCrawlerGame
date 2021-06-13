using UnityEngine;

namespace Dungen.World
{
    public readonly struct TileData
    {
        public readonly int x;
        public readonly int y;

        public TileData(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator Vector2Int(TileData data)
        {
            return new Vector2Int(data.x, data.y);
        }
    }
}
