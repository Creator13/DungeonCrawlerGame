using System;
using System.Collections.Generic;
using Dungen.Netcode.Protocol;
using Networking.Protocol;
using Unity.Jobs;
using Unity.Networking.Transport;
using UnityEngine;

namespace Networking.Client
{
    public abstract class Client
    {
        public enum ConnectionStatus { Connecting, Connected, Disconnected }

        protected NetworkDriver driver;
        protected NetworkConnection connection;
        private long timeSinceLastTransmit;
        protected JobHandle clientJobHandle;
        protected ConnectionStatus connected = ConnectionStatus.Connecting;

        private readonly Queue<Message> receiveQueue = new Queue<Message>();
        private readonly Queue<Message> sendQueue = new Queue<Message>();

        public MessageEvent[] callbacks = new MessageEvent[Enum.GetNames(typeof(MessageType)).Length];

        public string ConnectionIP { get; private set; }
        public bool Started { get; private set; }

        public ConnectionStatus Connected
        {
            get => connected;
            protected set
            {
                connected = value;
                ConnectionStatusChanged?.Invoke(value);
            }
        }

        public event Action<ConnectionStatus> ConnectionStatusChanged;

        public void Connect(string address = "")
        {
            if (Connected == ConnectionStatus.Connected)
            {
                LogError("Client already connected");
            }

            ConnectionIP = address;

            driver = NetworkDriver.Create();
            connection = default;

            NetworkEndPoint endpoint;
            if (!string.IsNullOrEmpty(address))
            {
                endpoint = NetworkEndPoint.Parse(address, NetConfig.PORT);
            }
            else
            {
                endpoint = NetworkEndPoint.LoopbackIpv4;
                endpoint.Port = NetConfig.PORT;
            }

            connection = driver.Connect(endpoint);

            for (var i = 0; i < callbacks.Length; i++)
            {
                callbacks[i] = new MessageEvent();
            }

            RegisterCallbacks();

            Started = true;
        }

        public void Disconnect()
        {
            clientJobHandle.Complete();

            if (Connected == ConnectionStatus.Disconnected)
            {
                LogWarning("Tried disconnecting while already disconnected!");
                return;
            }

            driver.Disconnect(connection);

            driver.ScheduleUpdate().Complete();

            Connected = ConnectionStatus.Disconnected;
        }

        public void Dispose()
        {
            clientJobHandle.Complete();

            if (Connected != ConnectionStatus.Disconnected)
            {
                Disconnect();
            }

            driver.Dispose();

            Started = false;
        }

        public void Update()
        {
            if (connection == default || Connected == ConnectionStatus.Disconnected) return;

            clientJobHandle.Complete();

            if (!connection.IsCreated)
            {
                // TODO let know that couldn't connect
                Log("Something went wrong while connecting");
                return;
            }

            if (Connected != ConnectionStatus.Connecting) KeepAlive();

            NetworkEvent.Type cmdType;
            while ((cmdType = connection.PopEvent(driver, out var reader)) != NetworkEvent.Type.Empty)
            {
                if (cmdType == NetworkEvent.Type.Data)
                {
                    timeSinceLastTransmit = 0;
                    HandleData(ref reader);
                }
                else if (cmdType == NetworkEvent.Type.Connect)
                {
                    Connected = ConnectionStatus.Connected;
                }
                else if (cmdType == NetworkEvent.Type.Disconnect)
                {
                    Log("Disconnected from server");
                    connection = default;
                    Connected = ConnectionStatus.Disconnected;
                }
            }

            ProcessMessageQueues();

            clientJobHandle = driver.ScheduleUpdate();
        }

        protected abstract void HandleData(ref DataStreamReader reader);

        protected abstract void RegisterCallbacks();

        protected void EnqueueReceived(Message msg)
        {
            receiveQueue.Enqueue(msg);
        }

        protected void Send(Message msg)
        {
            sendQueue.Enqueue(msg);
        }

        private void ProcessMessageQueues()
        {
            // Receive queue
            while (receiveQueue.Count > 0)
            {
                var msg = receiveQueue.Dequeue();
                callbacks[(int) msg.Type]?.Invoke(msg);
            }

            // Send queue
            while (sendQueue.Count > 0)
            {
                var msg = sendQueue.Dequeue();

                var res = driver.BeginSend(NetworkPipeline.Null, connection, out var writer);
                Message.Send(msg, ref writer);
                driver.EndSend(writer);
                timeSinceLastTransmit = 0;
            }
        }

        private void KeepAlive()
        {
            timeSinceLastTransmit += (long) (Time.deltaTime * 1000);

            if (timeSinceLastTransmit > NetConfig.KEEP_ALIVE_TIME)
            {
                Send(new NoneMessage());

                timeSinceLastTransmit = 0;
            }
        }


        #region Logging

        protected static void Log(string text)
        {
            Debug.Log($"CLIENT: {text}");
        }

        protected static void LogError(string text)
        {
            Debug.LogError($"CLIENT: {text}");
        }

        protected static void LogWarning(string text)
        {
            Debug.LogWarning($"CLIENT: {text}");
        }

        #endregion
    }
}
