using Unity.Networking.Transport;
using UnityEngine;

namespace Utils
{
    public static class DataStreamExtensions
    {
        public static void WriteVector2Int(this ref DataStreamWriter writer, Vector2Int vector2Int)
        {
            writer.WriteInt(vector2Int.x);
            writer.WriteInt(vector2Int.y);
        }
        
        public static Vector2Int ReadVector2Int(this ref DataStreamReader reader)
        {
            return new Vector2Int {
                x = reader.ReadInt(),
                y = reader.ReadInt(),
            };
        }
    }
}
