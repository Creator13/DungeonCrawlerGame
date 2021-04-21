using UnityEngine;

namespace Dungen
{
    [CreateAssetMenu(fileName = "New Settings", menuName = "Generator Settings")]
    public class GeneratorSettings : ScriptableObject
    {
        public int sizeX;
        public int sizeY;

        public bool useLabels;
        public RectTransform tileLabel;
    }
}
