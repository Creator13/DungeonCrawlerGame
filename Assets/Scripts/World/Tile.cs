using System.Collections.Generic;
using Dungen.Gameplay;
using UnityEngine;

namespace Dungen.World
{
    public class Tile : MonoBehaviour
    {
        private static readonly int hoverStrength = Shader.PropertyToID("_HoverStrength");
        private static readonly int baseColor = Shader.PropertyToID("_BaseColor");
        private MaterialPropertyBlock propertyBlock;

        private static readonly Color pathColor = new Color(255 / 255f, 173 / 255f, 83 / 255f);

        private new MeshRenderer renderer;

        private Color startColor;

        private List<IsoEntity> entities;

        public int X => Data.x;
        public int Y => Data.y;
        public TileData Data { get; private set; }

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetFloat(hoverStrength, 0);

            renderer = GetComponent<MeshRenderer>();
        }

        private void Start()
        {
            startColor = renderer.material.GetColor(baseColor);
        }

        public void Initialize(TileData data)
        {
            Data = data;
        }

        public void SetMarked(bool marked)
        {
            propertyBlock.SetFloat(hoverStrength, marked ? .4f : 0);

            renderer.SetPropertyBlock(propertyBlock);
        }

        public void SetShowPath(bool showPath)
        {
            propertyBlock.SetColor(baseColor, showPath ? pathColor : startColor);

            renderer.SetPropertyBlock(propertyBlock);
        }

        public void AddEntity(IsoEntity entity)
        {
            entities.Add(entity);
        }

        public void RemoveEntity(IsoEntity entity)
        {
            entities.Remove(entity);
        }

        public void GetDistributedPosition(int i) { }
    }
}
