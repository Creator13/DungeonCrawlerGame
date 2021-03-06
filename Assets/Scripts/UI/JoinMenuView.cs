using Dungen.Gameplay;
using Dungen.Highscore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dungen.UI
{
    public class JoinMenuView : MonoBehaviour
    {
        [SerializeField] private DungenGame gameController;

        [SerializeField] private Button newGameButton;
        [SerializeField] private Button joinGameButton;
        [SerializeField] private TMP_InputField ipInput;
        // [SerializeField] private TMP_InputField nameInput;

        private PlayerHighscoreHelper playerHighscoreHelper;

        public void Start()
        {
            joinGameButton.onClick.AddListener(JoinGame);
            newGameButton.onClick.AddListener(NewGame);

            playerHighscoreHelper = gameController.PlayerHighscoreHelper;
        }

        private void OnDisable()
        {
            joinGameButton.onClick.RemoveListener(JoinGame);
            newGameButton.onClick.RemoveListener(NewGame);
        }

        private void Update()
        {
            UpdateButtonsEnabled();
        }

        private void UpdateButtonsEnabled()
        {
            var ipEmpty = string.IsNullOrEmpty(ipInput.text);

            newGameButton.interactable = playerHighscoreHelper.LoggedIn;
            joinGameButton.interactable = playerHighscoreHelper.LoggedIn && !ipEmpty;
        }

        private void JoinGame()
        {
            gameController.ConnectToServer(ipInput.text);
        }

        private void NewGame()
        {
            gameController.StartServerAndConnect(ipInput.text);
        }
    }
}
