using System.Collections.Generic;

namespace Dungen.World
{
    public class GridGenerator
    {
        private readonly GeneratorSettings settings;
        
        public GridGenerator(GeneratorSettings settings)
        {
            this.settings = settings;
        }
        
        public TileData[] GenerateGrid()
        {
            var size = settings.sizeX * settings.sizeY;

            var tiles = new List<TileData>(settings.sizeX * settings.sizeY);

            for (var i = 0; i < size; i++)
            {
                var data = new TileData(i % settings.sizeY, i / settings.sizeX);
                tiles.Add(data);
            }

            return tiles.ToArray();
        }
    }
}
