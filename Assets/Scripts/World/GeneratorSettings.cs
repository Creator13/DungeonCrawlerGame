using UnityEngine;

namespace Dungen.World
{
    [CreateAssetMenu(fileName = "New Settings", menuName = "Generator Settings")]
    public class GeneratorSettings : ScriptableObject
    {
        public int sizeX;
        public int sizeY;

        public bool useLabels;
        public RectTransform tileLabel;
        public Tile tilePrefab;
    }
}
