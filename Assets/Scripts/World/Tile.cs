using System.Collections.Generic;
using System.Linq;
using Dungen.Gameplay;
using Gameplay.Entities;
using UnityEngine;

namespace Dungen.World
{
    public class Tile : MonoBehaviour
    {
        private static readonly int hoverStrength = Shader.PropertyToID("_HoverStrength");
        private static readonly int baseColor = Shader.PropertyToID("_BaseColor");
        private MaterialPropertyBlock propertyBlock;

        private static readonly Color pathColor = new Color(255 / 255f, 173 / 255f, 83 / 255f);
        private static readonly Color radiusColor = new Color(140 / 255f, 173 / 255f, 83 / 255f);

        private new MeshRenderer renderer;

        private Color startColor;

        private List<IsoEntity> entities = new List<IsoEntity>();

        public int X => Data.x;
        public int Y => Data.y;
        public TileData Data { get; private set; }

        public bool ContainsEnemy => entities.Any(entity => entity.GetComponent<Enemy>() != null);
        public bool ContainsPlayer =>
            entities.Any(entity =>
                entity.GetComponent<NetworkedPlayerController>() != null || entity.GetComponent<RemotePlayer>() != null);

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
        
        public void SetShowRadius(bool showPath)
        {
            propertyBlock.SetColor(baseColor, showPath ? radiusColor : startColor);

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
