using Dungen.Gameplay;
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
        [SerializeField] private TMP_InputField nameInput;

        public void Start()
        {
            joinGameButton.onClick.AddListener(JoinGame);
            newGameButton.onClick.AddListener(NewGame);
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
            var nameEmpty = string.IsNullOrEmpty(nameInput.text);
            var ipEmpty = string.IsNullOrEmpty(ipInput.text);

            newGameButton.interactable = !nameEmpty;
            joinGameButton.interactable = !nameEmpty && !ipEmpty;
        }

        private void JoinGame()
        {
            gameController.ConnectToServer(nameInput.text, ipInput.text);
        }

        private void NewGame()
        {
            gameController.StartServerAndConnect(nameInput.text, ipInput.text);
        }
    }
}
