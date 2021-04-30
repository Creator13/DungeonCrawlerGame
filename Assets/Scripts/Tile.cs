using UnityEngine;

namespace Dungen
{
    public class Tile : MonoBehaviour
    {
        private static readonly int BASE_COLOR = Shader.PropertyToID("_BaseColor");
        private  MaterialPropertyBlock propertyBlock;

        private static readonly Color pathColor = new Color(255 / 255f, 173 / 255f, 83 / 255f);
        
        private MeshRenderer renderer;
        
        private Color startColor;
        private TileData data;
        private static readonly int HOVER_STRENGTH = Shader.PropertyToID("_HoverStrength");

        public bool marked;
        
        public int X => data.x;
        public int Y => data.y;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetFloat(HOVER_STRENGTH, 0);
            
            renderer = GetComponent<MeshRenderer>();
            startColor = renderer.material.GetColor(BASE_COLOR);
        }

        public void Initialize(TileData data)
        {
            this.data = data;
        }

        public void SetMarked(bool marked)
        {
            this.marked = marked;
            
            propertyBlock.SetFloat(HOVER_STRENGTH, marked ? .4f : 0);

            renderer.SetPropertyBlock(propertyBlock);
        }

        public void SetShowPath(bool showPath)
        {
            propertyBlock.SetColor(BASE_COLOR, showPath ? pathColor : Color.white);
            
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
