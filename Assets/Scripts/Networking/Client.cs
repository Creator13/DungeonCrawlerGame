using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Jobs;
using Unity.Networking.Transport.Utilities;

namespace Networking
{
    public delegate void ClientMessageHandler(MessageHeader header);

    public delegate void ConnectionStatusDelegate(Client.ConnectionStatus newStatus);

    public abstract class Client
    {
        public enum ConnectionStatus { Disconnected, Connecting, Connected }

        private Dictionary<ushort, ClientMessageHandler> DefaultMessageHandlers =>
            new Dictionary<ushort, ClientMessageHandler> {
                {(ushort) BuiltinMessageTypes.Ping, HandlePing}
            };

        protected abstract Dictionary<ushort, ClientMessageHandler> NetworkMessageHandlers { get; }

        private Dictionary<ushort, Type> typeMap;

        private NetworkDriver driver;
        private NetworkPipeline pipeline;
        private NetworkConnection connection;

        private JobHandle jobHandle;

        private ConnectionStatus connected = ConnectionStatus.Disconnected;
        private float startTime = 0;

        // Public properties
        public string ConnectionIP { get; private set; }
        
        // Public events
        public event ConnectionStatusDelegate ConnectionStatusChanged;
        
        protected Client(IDictionary<ushort, Type> typeMap)
        {
            this.typeMap = new Dictionary<ushort, Type>(typeMap);
            NetworkMessageInfo.typeMap.ToList().ForEach(x => this.typeMap[x.Key] = x.Value);
        }

        public void Connect(string address = "", ushort port = 9000)
        {
            if (connected != ConnectionStatus.Disconnected)
            {
                Debug.LogWarning("Client already connected!");
                return;
            }

            ConnectionIP = address;

            startTime = Time.time;

            if (!driver.IsCreated)
            {
                driver = NetworkDriver.Create(new ReliableUtility.Parameters {WindowSize = 32});
                pipeline = driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
            }

            connection = default;

            NetworkEndPoint endpoint;
            if (!string.IsNullOrEmpty(ConnectionIP))
            {
                endpoint = NetworkEndPoint.Parse(ConnectionIP, port);
            }
            else
            {
                endpoint = NetworkEndPoint.LoopbackIpv4;
                endpoint.Port = port;
            }

            Debug.Log($"Connecting to {endpoint.Address}");

            connection = driver.Connect(endpoint);

            connected = ConnectionStatus.Connecting;
            ConnectionStatusChanged?.Invoke(connected);
        }

        public void Disconnect()
        {
            jobHandle.Complete();

            if (connected == ConnectionStatus.Disconnected)
            {
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
            connection = default;
            driver = default;
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
                ConnectionStatusChanged?.Invoke(connected);
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
                    ConnectionStatusChanged?.Invoke(connected);
                    OnConnected();
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    // First UInt is always message type (this is our own first design choice)
                    var msgType = reader.ReadUShort();

                    // TODO: Create message instance, and parse data...
                    var header = (MessageHeader) Activator.CreateInstance(typeMap[msgType]);
                    header.DeserializeObject(ref reader);

                    if (DefaultMessageHandlers.ContainsKey(msgType))
                    {
                        try
                        {
                            DefaultMessageHandlers[msgType].Invoke(header);
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
                            NetworkMessageHandlers[msgType].Invoke(header);
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
                    ConnectionStatusChanged?.Invoke(connected);
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

        private void HandlePing(MessageHeader header)
        {
            var pongMsg = new PongMessage();
            SendPackedMessage(pongMsg);
        }
    }
}
