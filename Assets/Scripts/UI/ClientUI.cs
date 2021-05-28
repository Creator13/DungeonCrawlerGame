using Dungen;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
public class ClientUI : MonoBehaviour {
    [SerializeField] private NetworkUIController gameController;

    [Space(10)] [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button connectButton;

    [Space(10)] [SerializeField] private ClientPanel clientPanelPrefab;
    [SerializeField] private RectTransform clientPanelContainer;

    private void Awake() {
        nameInput.onValueChanged.AddListener(SetConnectButtonInteractable);
        connectButton.onClick.AddListener(CreateNewClient);

        foreach (Transform item in clientPanelContainer) {
            Destroy(item.gameObject);
        }
    }

    private void Start() {
        SetConnectButtonInteractable();
    }

    private void OnDisable() {
        nameInput.onValueChanged.RemoveListener(SetConnectButtonInteractable);
        connectButton.onClick.RemoveListener(CreateNewClient);
    }

    private void CreateNewClient() {
        var client = NetworkUIController.CreateNewClient(nameInput.text);
        client.Connect(ipInput.text);

        var panel = Instantiate(clientPanelPrefab, clientPanelContainer, false);
        panel.Client = client;
    }

    private void SetConnectButtonInteractable(string value = "") {
        var interactable = !string.IsNullOrEmpty(nameInput.text);
        connectButton.interactable = interactable;
    }
}
}
