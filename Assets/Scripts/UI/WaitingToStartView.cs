using Dungen.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dungen.UI
{
    public class WaitingToStartView : MonoBehaviour
    {
        [SerializeField] private DungenGame gameController;
        
        [SerializeField] private TMP_Text players;
        [SerializeField] private Button startButton;

        private void Start()
        {
            startButton.onClick.AddListener(OnClickStart);
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveListener(OnClickStart);
        }

        private void Update()
        {
            var playerText = "";
            foreach (var player in gameController.Players.Values)
            {
                playerText += $"{player.name} ({player.networkId})\n";
            }

            players.text = playerText;
        }

        private void OnClickStart()
        {
            gameController.Client.RequestGameStart();
        }
    }
}
