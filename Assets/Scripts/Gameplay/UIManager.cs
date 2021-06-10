using System;
using Dungen.UI;
using UnityEngine;

namespace Dungen.Gameplay
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Modal modal;
        [SerializeField] private JoinMenuView joinMenuView;
        [SerializeField] private WaitingToStartView waitingToStartView;

        public Modal Modal => modal;
        public JoinMenuView JoinMenuView => joinMenuView;
        public WaitingToStartView WaitingToStartView => waitingToStartView;

        private void Awake()
        {
            modal.HideModal();
            
            joinMenuView.gameObject.SetActive(false);
            waitingToStartView.gameObject.SetActive(false);
        }
    }
}
