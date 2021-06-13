using UnityEngine;

namespace Dungen.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Modal modal;
        [SerializeField] private JoinMenuView joinMenuView;
        [SerializeField] private WaitingToStartView waitingToStartView;
        [SerializeField] private GameHudView gameHudView;

        public Modal Modal => modal;
        public JoinMenuView JoinMenuView => joinMenuView;
        public WaitingToStartView WaitingToStartView => waitingToStartView;
        public GameHudView GameHudView => gameHudView;

        private void Awake()
        {
            modal.HideModal();
            
            joinMenuView.gameObject.SetActive(false);
            waitingToStartView.gameObject.SetActive(false);
            gameHudView.gameObject.SetActive(false);
        }
    }
}
