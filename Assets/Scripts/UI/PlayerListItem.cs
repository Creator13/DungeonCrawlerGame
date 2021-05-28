using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Player
    {
        public int id;
        public bool active;

        public Utils.Color color;
        public string name;

        public override string ToString()
        {
            return $"Player[ID:{id}, name:{name}, color:{color}]";
        }
    }

    internal class PlayerListItem : MonoBehaviour
    {
        [SerializeField] private Image connectionIndicator;
        [SerializeField] private Image playerColor;
        [SerializeField] private TMP_Text playerId;
        [SerializeField] private TMP_Text playerName;

        public Player Player
        {
            set
            {
                connectionIndicator.color = value.active ? Color.green : Color.red;
                playerColor.color = value.color;
                playerId.text = value.id.ToString();
                playerName.text = value.name;
            }
        }
    }
}
