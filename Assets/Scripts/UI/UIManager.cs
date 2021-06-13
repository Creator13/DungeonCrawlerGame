using UnityEngine;

namespace Dungen.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Modal modal;
        [SerializeField] private JoinMenuView joinMenuView;
        [SerializeField] private WaitingToStartView waitingToStartView;
        [SerializeField] private GameHudView gameHudView;
        [SerializeField] private GameOverView gameOverView;

        public Modal Modal => modal;
        public JoinMenuView JoinMenuView => joinMenuView;
        public WaitingToStartView WaitingToStartView => waitingToStartView;
        public GameHudView GameHudView => gameHudView;
        public GameOverView GameOverView => gameOverView;

        private void Awake()
        {
            modal.HideModal();

            joinMenuView.gameObject.SetActive(false);
            waitingToStartView.gameObject.SetActive(false);
            gameHudView.gameObject.SetActive(false);
            gameOverView.gameObject.SetActive(false);
        }
    }
}
