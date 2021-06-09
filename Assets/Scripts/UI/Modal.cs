using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dungen.UI
{
    public class Modal : MonoBehaviour
    {
        [Flags]
        public enum ModalDialogAction { Confirm = 0x1, Cancel = 0x2 };

        public delegate void ModalDialogCallback(ModalDialogAction choice);

        [SerializeField] private GameObject background;
        [SerializeField] private GameObject dialog;

        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text body;
        [SerializeField] private Transform buttonBar;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private void Awake()
        {
            HideModal();
        }

        public void ShowModal(ModalDialogAction actions, string title, string message, ModalDialogCallback callback = null)
        {
            foreach (Transform t in buttonBar)
            {
                t.gameObject.SetActive(false);
            }

            if ((actions & ModalDialogAction.Cancel) != 0)
            {
                cancelButton.gameObject.SetActive(true);
                confirmButton.onClick.AddListener(() =>
                {
                    HideModal();
                    callback?.Invoke(ModalDialogAction.Cancel);
                });
            }

            if ((actions & ModalDialogAction.Confirm) != 0)
            {
                confirmButton.gameObject.SetActive(true);
                confirmButton.onClick.AddListener(() =>
                {
                    HideModal();
                    callback?.Invoke(ModalDialogAction.Confirm);
                });
            }

            this.title.text = title;
            body.text = message;

            background.SetActive(true);
            dialog.SetActive(true);
        }

        public void HideModal()
        {
            background.SetActive(false);
            dialog.SetActive(false);

            cancelButton.onClick.RemoveAllListeners();
            confirmButton.onClick.RemoveAllListeners();
        }
    }
}
