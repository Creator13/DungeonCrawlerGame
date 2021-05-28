using Dungen;
using Dungen.Netcode;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
public class ServerUI : MonoBehaviour {
    [SerializeField] private Image runningIndicator;
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;

    [Space(10)] [SerializeField] private ConnectionItem connectionListPrefab;
    [SerializeField] private Transform connectionListContainer;
    [SerializeField] private TMP_Text connectionCounter;

    [Space(10)] [SerializeField] private PlayerListItem playerListPrefab;
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private TMP_Text playerCounter;

    [Space(10)] [SerializeField] private TMP_InputField commandField;

    [Space(10)] [SerializeField] private NetworkUIController gameController;
    private DungenServer server;

    private void Awake() {
        if (!gameController) {
            gameController = FindObjectOfType<NetworkUIController>();
        }

        foreach (Transform item in connectionListContainer) {
            Destroy(item.gameObject);
        }

        foreach (Transform item in playerListContainer) {
            Destroy(item.gameObject);
        }
    }

    private void Start() {
        server = gameController.Server;

        startButton.onClick.AddListener(StartServer);
        stopButton.onClick.AddListener(StopServer);

        // commandField.onSubmit.AddListener(PerformCommand);

        server.ConnectionsUpdated += UpdateConnectionList;
        server.RunningStateChanged += UpdateIndicator;
        server.RunningStateChanged += UpdateButtonsActive;
        server.RunningStateChanged += UpdateConnectionList;

        server.lobby.PlayersUpdated += UpdatePlayerList;

        Init();
    }

    private void OnDisable() {
        startButton.onClick.RemoveListener(StartServer);
        stopButton.onClick.RemoveListener(StopServer);

        // commandField.onSubmit.RemoveListener(PerformCommand);

        server.ConnectionsUpdated -= UpdateConnectionList;
        server.RunningStateChanged -= UpdateIndicator;
        server.RunningStateChanged -= UpdateButtonsActive;
        server.RunningStateChanged -= UpdateConnectionList;

        server.lobby.PlayersUpdated -= UpdatePlayerList;
    }

    private void Init() {
        UpdateIndicator(server.IsRunning);
        UpdateButtonsActive(server.IsRunning);
    }

    private void UpdateIndicator(bool state) {
        runningIndicator.color = state ? Color.green : Color.red;
    }

    private void UpdateButtonsActive(bool state) {
        startButton.interactable = !state;
        stopButton.interactable = state;
    }

    private void UpdateConnectionList() {
        foreach (Transform item in connectionListContainer) {
            Destroy(item.gameObject);
        }

        foreach (var conn in server.Connections) {
            var listItem = Instantiate(connectionListPrefab, connectionListContainer, false);
            listItem.ID = conn.InternalId;
        }

        connectionCounter.text = $"{server.Connections.Count}/{server.MaxConnections}";
    }

    private void UpdateConnectionList(bool state) {
        UpdateConnectionList();
    }

    private void UpdatePlayerList() {
        foreach (Transform item in playerListContainer) {
            Destroy(item.gameObject);
        }

        foreach (var player in server.lobby.players) {
            var listItem = Instantiate(playerListPrefab, playerListContainer, false);
            listItem.Player = player;
        }

        playerCounter.text = $"{server.lobby.players.Count}/{server.lobby.MaxPlayers}";
        playerCounter.color = server.lobby.players.Count > server.lobby.MaxPlayers ? Color.red : Color.white;
    }

    // private void PerformCommand(string command) {
    //     server.PerformCommand(command);
    //     commandField.text = "";
    // }

    private void StartServer() {
        server.Start();
    }

    private void StopServer() {
        server.Stop();
    }
}
}
