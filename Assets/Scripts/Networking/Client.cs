using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Jobs;
using Unity.Networking.Transport.Utilities;

namespace Networking
{
    public delegate void ClientMessageHandler(Client client, MessageHeader header);

    public abstract class Client
    {
        private enum ConnectionStatus {Disconnected, Connecting, Connected}
        
        private static Dictionary<ushort, ClientMessageHandler> DefaultMessageHandlers =
            new Dictionary<ushort, ClientMessageHandler> {
                {(ushort) BuiltinMessageTypes.Ping, HandlePing}
            };

        protected abstract Dictionary<ushort, ClientMessageHandler> NetworkMessageHandlers { get; }

        private NetworkDriver driver;
        private NetworkPipeline pipeline;
        private NetworkConnection connection;

        private JobHandle jobHandle;

        private ConnectionStatus connected = ConnectionStatus.Disconnected;
        private float startTime = 0;

        // Start is called before the first frame update
        public void Connect(string address = "", ushort port = 9000)
        {
            startTime = Time.time;

            driver = NetworkDriver.Create(new ReliableUtility.Parameters {WindowSize = 32});
            pipeline = driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

            connection = default;

            NetworkEndPoint endpoint;
            if (!string.IsNullOrEmpty(address)) {
                endpoint = NetworkEndPoint.Parse(address, port);
            }
            else {
                endpoint = NetworkEndPoint.LoopbackIpv4;
                endpoint.Port = port;
            }
            
            Debug.Log($"Connecting to {endpoint.Address}");
            
            connection = driver.Connect(endpoint);

            connected = ConnectionStatus.Connecting;
        }

        // // No collections list this time...
        // private void OnApplicationQuit()
        // {
        //     Disconnect();
        // }

        public void Disconnect()
        {
            jobHandle.Complete();

            if (connected == ConnectionStatus.Disconnected) {
                Debug.LogWarning("Tried disconnecting while already disconnected!");
                return;
            }
            
            if (connection.IsCreated)
            {
                connection.Disconnect(driver);
            }
            
            driver.ScheduleUpdate().Complete();

            connected = ConnectionStatus.Disconnected;
        }

        public void Dispose()
        {
            jobHandle.Complete();

            if (connected != ConnectionStatus.Disconnected)
            {
                Disconnect();
            }
            
            driver.Dispose();
        }

        public void Update()
        {
            if (connected == ConnectionStatus.Disconnected) return;
            
            jobHandle.Complete();

            // TODO This code handles timeout
            if (connected == ConnectionStatus.Connecting && Time.time - startTime > 5f)
            {
                Debug.Log("Failed to connect! Timed out");
                connected = ConnectionStatus.Disconnected;
            }

            if (!connection.IsCreated)
            {
                Debug.Log("Something went wrong during connect");
                return;
            }

            NetworkEvent.Type cmd;
            while ((cmd = connection.PopEvent(driver, out var reader)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    Debug.Log("Connected!");
                    connected = ConnectionStatus.Connected;
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    // First UInt is always message type (this is our own first design choice)
                    var msgType = reader.ReadUShort();

                    // TODO: Create message instance, and parse data...
                    var header = (MessageHeader) Activator.CreateInstance(NetworkMessageInfo.typeMap[msgType]);
                    header.DeserializeObject(ref reader);

                    if (DefaultMessageHandlers.ContainsKey(msgType))
                    {
                        try
                        {
                            DefaultMessageHandlers[msgType].Invoke(this, header);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Malformed message received: {msgType}\n{e}");
                        }
                    }
                    
                    if (NetworkMessageHandlers.ContainsKey(msgType))
                    {
                        try
                        {
                            NetworkMessageHandlers[msgType].Invoke(this, header);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Malformed message received: {msgType}\n{e}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Unsupported message type received: {msgType}");
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client got disconnected from server");
                    connection = default;
                    connected = ConnectionStatus.Disconnected;
                }
            }

            jobHandle = driver.ScheduleUpdate();
        }

        protected abstract void OnConnected();
        
        public void SendPackedMessage(MessageHeader header)
        {
            var result = driver.BeginSend(pipeline, connection, out var writer);

            // non-0 is an error code
            if (result == 0)
            {
                header.SerializeObject(ref writer);
                driver.EndSend(writer);
            }
            else
            {
                Debug.LogError($"Could not write message to driver (error code {result})");
            }
        }

        private static void HandlePing(Client client, MessageHeader header) {
            var pongMsg = new PongMessage();
            client.SendPackedMessage(pongMsg);
        }
    }
}
