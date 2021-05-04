using System;
using System.Collections.Generic;
using System.Linq;
using Dungen.Netcode.Protocol;
using Networking.Protocol;
using Unity.Collections;
using Unity.Jobs;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Events;

namespace Networking.Server
{
    public abstract class Server
    {
        public struct MessageWrapper
        {
            public Message message;
            public int senderId;
        }

        public class TraceableMessageEvent : UnityEvent<MessageWrapper> { }

        private NetworkDriver driver;
        private NativeList<NetworkConnection> connections;
        private JobHandle jobHandle;

        private readonly Queue<MessageWrapper> receiveQueue = new Queue<MessageWrapper>();
        private readonly Queue<MessageWrapper> sendQueue = new Queue<MessageWrapper>();

        private readonly List<int> kickSchedule = new List<int>();

        protected List<Action> updateMethods = new List<Action>();

        public int MaxConnections => NetConfig.MAX_CONNECTIONS;
        public ushort Port => NetConfig.PORT;

        public TraceableMessageEvent[] callbacks = new TraceableMessageEvent[Enum.GetNames(typeof(MessageType)).Length];

        private bool isRunning;

        public bool IsRunning
        {
            get => isRunning;
            private set
            {
                isRunning = value;
                RunningStateChanged?.Invoke(value);
            }
        }

        public List<NetworkConnection> Connections
        {
            get
            {
                if (connections.IsCreated)
                {
                    return connections.ToArray().ToList();
                }

                return new List<NetworkConnection>();
            }
        }

        public event Action<bool> RunningStateChanged;
        public event Action ConnectionsUpdated;
        public event Action<int> ConnectionReceived;
        public event Action<int> ConnectionRemoved;

        public bool Start()
        {
            if (IsRunning)
            {
                LogWarning("Server already running!");
                return false;
            }

            // Create server
            driver = NetworkDriver.Create();
            var endpoint = NetworkEndPoint.AnyIpv4;
            endpoint.Port = Port;

            // Bind to port
            if (driver.Bind(endpoint) != 0)
            {
                LogError($"Failed to bind port {Port}.");
                return false;
            }

            connections = new NativeList<NetworkConnection>(NetConfig.MAX_CONNECTIONS, Allocator.Persistent);

            // Create callbacks
            for (var i = 0; i < callbacks.Length; i++)
            {
                callbacks[i] = new TraceableMessageEvent();
            }

            driver.Listen();

            IsRunning = true;

            Log($"Listening on port {Port}.");

            ConnectionRemoved += id => Log($"Client disconnected from server. ID was {id}.");

            RegisterCallbacks();

            return true;
        }

        protected abstract void RegisterCallbacks();

        public void Stop()
        {
            if (!IsRunning) return;

            jobHandle.Complete();

            for (var i = 0; i < connections.Length; i++)
            {
                driver.Disconnect(connections[i]);
                ConnectionRemoved?.Invoke(connections[i].InternalId);
                connections[i] = default;
            }

            driver.ScheduleUpdate().Complete();

            driver.Dispose();
            connections.Dispose();

            IsRunning = false;

            Clean();

            Log("Stopped server.");
        }

        protected abstract void Clean();

        public void Update()
        {
            jobHandle.Complete();

            foreach (var id in kickSchedule)
            {
                Kick(id);
            }

            // Clean up connections
            for (var i = 0; i < connections.Length; i++)
            {
                if (connections[i].GetState(driver) == NetworkConnection.State.Disconnected)
                {
                    ConnectionRemoved?.Invoke(connections[i].InternalId);
                    connections.RemoveAtSwapBack(i);
                    ConnectionsUpdated?.Invoke();
                    i++;
                }
            }

            // Accept connections
            NetworkConnection newConnection;
            while ((newConnection = driver.Accept()) != default)
            {
                connections.Add(newConnection);
                ConnectionsUpdated?.Invoke();
                ConnectionReceived?.Invoke(newConnection.InternalId);
            }

            for (var i = 0; i < connections.Length; i++)
            {
                if (!connections[i].IsCreated)
                {
                    continue;
                }

                NetworkEvent.Type cmd;
                while ((cmd = driver.PopEventForConnection(connections[i], out var reader)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                        HandleData(ref reader, connections[i].InternalId);
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        connections[i].Close(driver);
                    }
                }
            }

            ProcessMessageQueues();

            jobHandle = driver.ScheduleUpdate();

            foreach (var method in updateMethods.ToArray())
            {
                method?.Invoke();
            }
        }

        protected abstract void HandleData(ref DataStreamReader reader, int id);

        public void ScheduleKick(int connId)
        {
            if (!kickSchedule.Contains(connId))
            {
                kickSchedule.Add(connId);
            }
        }

        private void Kick(int connId)
        {
            try
            {
                var i = Connections.IndexOf(Connections.First(conn => conn.InternalId == connId));

                Log($"Kicking connection with id {connId}.");
                connections[i].Disconnect(driver);
                connections[i].Close(driver);
            }
            catch (InvalidOperationException e)
            {
                LogError($"Tried kicking connection, but no connection with id {connId} was found.");
            }

            kickSchedule.Remove(connId);
        }

        protected void EnqueueReceived(Message msg, int connId)
        {
            receiveQueue.Enqueue(new MessageWrapper
            {
                message = msg,
                senderId = connId
            });
        }

        public void Send(Message msg, int recipientId)
        {
            sendQueue.Enqueue(new MessageWrapper
            {
                message = msg,
                senderId = recipientId
            });
        }

        public void SendToAllExcluding(Message msg, int excludingId)
        {
            foreach (var connection in connections)
            {
                if (connection.InternalId == excludingId) continue;

                sendQueue.Enqueue(new MessageWrapper
                {
                    message = msg,
                    senderId = connection.InternalId
                });
            }
        }

        public void SendToAll(Message msg)
        {
            foreach (var connection in connections)
            {
                sendQueue.Enqueue(new MessageWrapper
                {
                    message = msg,
                    senderId = connection.InternalId
                });
            }
        }

        public void SendToAll(Message msg, params int[] ids)
        {
            foreach (var id in ids)
            {
                sendQueue.Enqueue(new MessageWrapper
                {
                    message = msg,
                    senderId = id
                });
            }
        }

        protected void KeepAlive(int connId)
        {
            var msg = new NoneMessage();
            Send(msg, connId);
        }

        private void ProcessMessageQueues()
        {
            while (receiveQueue.Count > 0)
            {
                var msg = receiveQueue.Dequeue();
                callbacks[(int) msg.message.Type]?.Invoke(msg);
            }

            while (sendQueue.Count > 0)
            {
                // TODO upgrade this so that all messages can be sent to the client at once without any more lookups than necessary
                var connectionsTemp = Connections;
                var msg = sendQueue.Dequeue();
                var idExists = connectionsTemp.Any(conn => conn.InternalId == msg.senderId);
                if (idExists)
                {
                    var connection = connectionsTemp.First(conn => conn.InternalId == msg.senderId);

                    var res = driver.BeginSend(NetworkPipeline.Null, connection, out var writer);
                    Message.Send(msg.message, ref writer);
                    driver.EndSend(writer);
                }
                else
                {
                    LogError(
                        $"Tried sending {msg.message.GetType()} to player {msg.senderId}, but id could not be found!");
                }
            }
        }


        #region Logging

        public void Log(string text)
        {
            Debug.Log($"SERVER:{Port}: {text}");
        }

        public void LogError(string text)
        {
            Debug.LogError($"SERVER:{Port}: {text}");
        }

        public void LogWarning(string text)
        {
            Debug.LogWarning($"SERVER:{Port}: {text}");
        }

        #endregion
    }
}
