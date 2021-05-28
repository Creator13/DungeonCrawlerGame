using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerInfo
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public Color Color { get; set; }
    }

    internal class PlayerListItemClient : MonoBehaviour
    {
        [SerializeField] private Image playerColor;
        [SerializeField] private TMP_Text playerId;
        [SerializeField] private TMP_Text playerName;

        public PlayerInfo Player
        {
            set
            {
                playerColor.color = value.Color;
                playerId.text = value.ID.ToString();
                playerName.text = value.Name;
            }
        }
    }
}
