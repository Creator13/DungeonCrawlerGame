using UnityEngine;

namespace Dungen.World
{
    [RequireComponent(typeof(IsoGrid))]
    public class GeneratorBehaviour : MonoBehaviour
    {
        [SerializeField] private GeneratorSettings gridGeneratorSettings;
        [SerializeField] private IsoGrid grid;

        private void Awake()
        {
            if (!grid) grid = GetComponent<IsoGrid>();
        }

        private void Start()
        {
            grid.CreateGridFromTileDataArray(new GridGenerator(gridGeneratorSettings).GenerateGrid());
        }
    }
}
