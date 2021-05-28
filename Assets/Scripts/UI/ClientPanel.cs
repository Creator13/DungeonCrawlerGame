using Dungen.Netcode;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ClientPanel : MonoBehaviour
    {
        [SerializeField] private Image connectionIndicator;
        [SerializeField] private Image playerColor;
        [SerializeField] private TMP_Text playerName;
        [SerializeField] private TMP_Text ipDisplay;
        [SerializeField] private Button disconnectButton;
        [SerializeField] private Button removeButton;

        [Space(10)] [SerializeField] private TMP_Text playerId;

        [Space(10)] [SerializeField] private PlayerListItemClient otherPlayerListPrefab;
        [SerializeField] private RectTransform otherPlayerListContainer;

        private string ConnectionIp
        {
            set => ipDisplay.text = value == string.Empty ? "127.0.0.1" : value;
        }

        private uint PlayerId
        {
            set => playerId.text = $"Player number: {value}";
        }

        private Color PlayerColor
        {
            set => playerColor.color = value;
        }

        private DungenClient client;
        private ClientBehavior clientBehaviour;

        public ClientBehavior Client
        {
            set
            {
                if (value.Client != null)
                {
                    ConnectionIp = value.Client.ConnectionIP;
                    value.Client.ConnectionStatusChanged += UpdateConnectionStatus;
                    // value.Client.OtherPlayersChanged += UpdateOtherPlayerList; TODO
                    removeButton.onClick.AddListener(Remove);
                    disconnectButton.onClick.AddListener(Disconnect);
                }

                client = value.Client;
                clientBehaviour = value;
            }
        }

        private void OnDestroy()
        {
            if (client != null)
            {
                client.ConnectionStatusChanged -= UpdateConnectionStatus;
            }

            removeButton.onClick.RemoveListener(Remove);
            disconnectButton.onClick.RemoveListener(Disconnect);
        }

        private void UpdateConnectionStatus(Client.ConnectionStatus status)
        {
            switch (status)
            {
                case Networking.Client.ConnectionStatus.Connecting:
                    disconnectButton.interactable = false;
                    playerName.text = "Connecting...";
                    connectionIndicator.color = new Color(252f / 255f, 115f / 255f, 3f / 255f);
                    break;
                case Networking.Client.ConnectionStatus.Connected:
                    disconnectButton.interactable = true;
                    playerName.text = $"CLIENT: {client.PlayerName}";
                    PlayerId = client.NetworkID;
                    connectionIndicator.color = Color.green;
                    break;
                case Networking.Client.ConnectionStatus.Disconnected:
                    disconnectButton.interactable = false;
                    playerName.text = "Disconnected";
                    connectionIndicator.color = Color.red;
                    break;
            }
        }

        // TODO
        // private void UpdateOtherPlayerList() {
        //     foreach (Transform item in otherPlayerListContainer) {
        //         Destroy(item.gameObject);
        //     }
        //
        //     foreach (var player in client.OtherPlayers) {
        //         var item = Instantiate(otherPlayerListPrefab, otherPlayerListContainer, false);
        //         item.Player = player;
        //     }
        // }

        private void Remove()
        {
            Destroy(gameObject);
            Destroy(clientBehaviour.gameObject);
        }

        private void Disconnect()
        {
            client.Disconnect();
        }
    }
}
