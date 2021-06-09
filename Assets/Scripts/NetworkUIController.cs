using Dungen.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace Dungen
{
    public class NetworkUIController : MonoBehaviour
    {
        // public DungenServer Server { get; private set; }
        //
        // private void Awake()
        // {
        //     Server = new DungenServer(1511);
        // }
        //
        // private void Update()
        // {
        //     Server?.Update();
        // }
        //
        // private void OnDisable()
        // {
        //     Server.Stop();
        // }
        //
        // public static ClientBehavior CreateNewClient(string playerName)
        // {
        //     Assert.IsFalse(string.IsNullOrEmpty(playerName));
        //
        //     var clientObject = new GameObject("Client");
        //     var clientBehaviour = clientObject.AddComponent<ClientBehavior>();
        //     clientBehaviour.Initialize(playerName);
        //
        //     return clientBehaviour;
        // }
    }
}
