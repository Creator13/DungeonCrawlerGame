using Dungen.Gameplay;
using TMPro;
using UnityEngine;

namespace Dungen.UI
{
    public class GameHudView : MonoBehaviour
    {
        [SerializeField] private DungenGame game;

        [SerializeField] private TMP_Text turnDisplay;
        
        private void Update()
        {
            var player = game.CurrentPlayerTurn;
            
            turnDisplay.SetText($"It's {(player.networkId == game.Client.OwnNetworkId ? "your" : $"{player.name}'s")} turn!");
        }
    }
}
