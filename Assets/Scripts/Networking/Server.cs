using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using Unity.Jobs;
using Unity.Networking.Transport.Utilities;
using Utils;

namespace Networking
{
    public delegate void ServerMessageHandler(NetworkConnection connection, MessageHeader header);

    public enum BuiltinMessageTypes : ushort { Ping = 60001, Pong = 60002 }

    public class KeepAliveStatus
    {
        public float lastSendTime;
        public bool receivedReplySinceLast;
    }

    public abstract class Server
    {
        private const int CAPACITY = 32;
        private const float KEEP_ALIVE_TIMEOUT = 5f;

        private readonly ushort port;

        private JobHandle jobHandle;
        private NetworkDriver driver;
        private NetworkPipeline pipeline;
        private NativeList<NetworkConnection> connections;

        private List<NetworkConnection> playableConnections;

        private Dictionary<NetworkConnection, KeepAliveStatus> keepAliveStatusMap =
            new Dictionary<NetworkConnection, KeepAliveStatus>();

        private Dictionary<ushort, Type> fullTypeMap;

        private Dictionary<ushort, ServerMessageHandler> DefaultMessageHandlers =>
            new Dictionary<ushort, ServerMessageHandler> {
                {(ushort) BuiltinMessageTypes.Pong, HandleClientPong}
            };

        protected abstract Dictionary<ushort, ServerMessageHandler> NetworkMessageHandlers { get; }

        // Public events
        public event Action<bool> RunningStateChanged;
        public event Action ConnectionsUpdated;
        public event Action<NetworkConnection> ConnectionRemoved;

        // Public properties
        public bool IsRunning { get; private set; }
        public List<NetworkConnection> Connections => connections.ToArray().ToList();
        public int MaxConnections => CAPACITY;

        protected Server(ushort port, IDictionary<ushort, Type> typeMap)
        {
            this.port = port;

            // Combine the user-defined type map with the technical/builtin map
            fullTypeMap = new Dictionary<ushort, Type>(typeMap);
            NetworkMessageInfo.typeMap.ToList().ForEach(x => fullTypeMap[x.Key] = x.Value);
        }

        public bool Start()
        {
            // Create Driver
            driver = NetworkDriver.Create(new ReliableUtility.Parameters {WindowSize = CAPACITY});
            pipeline = driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

            // Open listener on server port
            var endpoint = NetworkEndPoint.AnyIpv4;
            endpoint.Port = port;
            if (driver.Bind(endpoint) != 0)
            {
                Debug.Log($"Failed to bind to port {port}");
                return false;
            }

            connections = new NativeList<NetworkConnection>(CAPACITY, Allocator.Persistent);

            driver.Listen();

            IsRunning = true;
            RunningStateChanged?.Invoke(IsRunning);

            Debug.Log($"Started server on port {port}!");

            return true;
        }

        public void Stop()
        {
            if (!IsRunning) return;

            IsRunning = false;
            RunningStateChanged?.Invoke(false);

            jobHandle.Complete();

            for (var i = 0; i < connections.Length; i++)
            {
                DisconnectClient(connections[i]);
            }

            driver.ScheduleUpdate().Complete();

            driver.Dispose();
            connections.Dispose();

            Debug.Log($"Stopped server on port {port}.");
        }

        public void Update()
        {
            if (!IsRunning) return;

            jobHandle.Complete();

            // Clean up connections, remove stale ones
            for (var i = 0; i < connections.Length; i++)
            {
                if (!connections[i].IsCreated ||
                    connections[i].GetState(driver) == NetworkConnection.State.Disconnected)
                {
                    ConnectionRemoved?.Invoke(connections[i]);
                    connections.RemoveAtSwapBack(i);
                    --i;
                    ConnectionsUpdated?.Invoke();
                }
            }

            // Accept new connections
            NetworkConnection c;
            while ((c = driver.Accept()) != default)
            {
                connections.Add(c);
                AcceptConnection(c);
                ConnectionsUpdated?.Invoke();
            }

            foreach (var connection in connections)
            {
                if (!connection.IsCreated) continue;

                // Loop through available events
                NetworkEvent.Type cmd;
                while ((cmd = driver.PopEventForConnection(connection, out var reader)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                        ReadDataAsMessage(connection, reader);
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        connection.Close(driver);
                    }
                }
            }

            // // Ping Pong stuff for timeout disconnects
            // for (var i = 0; i < connections.Length; i++)
            // {
            //     if (!connections[i].IsCreated)
            //         continue;
            //
            //     if (pongDict.ContainsKey(connections[i]))
            //     {
            //         if (Time.time - pongDict[connections[i]].lastSendTime > 5f)
            //         {
            //             pongDict[connections[i]].lastSendTime = Time.time;
            //             if (pongDict[connections[i]].status == 0)
            //             {
            //                 // Remove from all the dicts, save name / id for msg
            //
            //                 // FIXME: for some reason, sometimes this isn't in the list?
            //                 if (nameList.ContainsKey(connections[i]))
            //                 {
            //                     nameList.Remove(connections[i]);
            //                 }
            //
            //                 uint destroyId = playerInstances[connections[i]].networkId;
            //                 networkManager.DestroyWithId(destroyId);
            //                 playerInstances.Remove(connections[i]);
            //
            //                 var name = pongDict[connections[i]].name;
            //                 pongDict.Remove(connections[i]);
            //
            //                 // Disconnect this player
            //                 connections[i].Disconnect(driver);
            //
            //                 // Build messages
            //                 var msg = $"{name} has been Disconnected (connection timed out)";
            //                 chat.NewMessage(msg, ChatCanvas.leaveColor);
            //
            //                 ChatMessage quitMsg = new ChatMessage {
            //                     message = msg,
            //                     messageType = MessageType.Quit
            //                 };
            //
            //                 DestroyMessage destroyMsg = new DestroyMessage {
            //                     networkId = destroyId
            //                 };
            //
            //                 // Broadcast
            //                 SendBroadcast(quitMsg);
            //                 SendBroadcast(destroyMsg, connections[i]);
            //
            //                 // Clean up
            //                 connections[i] = default;
            //             }
            //             else
            //             {
            //                 pongDict[connections[i]].status -= 1;
            //                 PingMessage pingMsg = new PingMessage();
            //                 SendUnicast(connections[i], pingMsg);
            //             }
            //         }
            //     }
            //     else if (nameList.ContainsKey(connections[i]))
            //     {
            //         //means they've succesfully handshaked
            //         var ping = new PingPong();
            //         ping.lastSendTime = Time.time;
            //         ping.status = 3; // 3 retries
            //         ping.name = nameList[connections[i]];
            //         pongDict.Add(connections[i], ping);
            //
            //         PingMessage pingMsg = new PingMessage();
            //         SendUnicast(connections[i], pingMsg);
            //     }
            // }

            PingClients();
            
            jobHandle = driver.ScheduleUpdate();
        }

        private void ReadDataAsMessage(NetworkConnection connection, DataStreamReader reader)
        {
            var msgType = reader.ReadUShort();

            var header = (MessageHeader) Activator.CreateInstance(fullTypeMap[msgType]);
            header.DeserializeObject(ref reader);

            var hasKey = false;
            // First execute default handlers to ensure proper functioning of server & client connection
            if (DefaultMessageHandlers.ContainsKey(msgType))
            {
                hasKey = true;
                try
                {
                    DefaultMessageHandlers[msgType].Invoke(connection, header);
                }
                catch (KeyNotFoundException e)
                {
                    Debug.LogError($"Badly formatted message received: {msgType}\n{e.StackTrace}");
                }
            }

            // Then execute game-specific handlers
            if (NetworkMessageHandlers.ContainsKey(msgType))
            {
                hasKey = true;
                try
                {
                    NetworkMessageHandlers[msgType].Invoke(connection, header);
                }
                catch (KeyNotFoundException e)
                {
                    Debug.LogError($"Badly formatted message received: {msgType}\n{e.StackTrace}");
                }
            }
            
            if (!hasKey)
            {
                Debug.LogWarning($"Unsupported message type received: {msgType}");
            }
        }

        protected virtual void AcceptConnection(NetworkConnection connection) { }

        public void SendUnicast(NetworkConnection connection, MessageHeader header, bool reliable = true)
        {
            var result = driver.BeginSend(reliable ? pipeline : NetworkPipeline.Null, connection, out var writer);
            if (result == 0)
            {
                header.SerializeObject(ref writer);
                driver.EndSend(writer);
            }
        }

        public void SendBroadcast(MessageHeader header, NetworkConnection toExclude = default, bool reliable = true)
        {
            for (var i = 0; i < connections.Length; i++)
            {
                if (!connections[i].IsCreated || connections[i] == toExclude)
                    continue;

                var result = driver.BeginSend(reliable ? pipeline : NetworkPipeline.Null, connections[i],
                    out var writer);
                if (result == 0)
                {
                    header.SerializeObject(ref writer);
                    driver.EndSend(writer);
                }
            }
        }

        private void PingClients()
        {
            for (var i = 0; i < connections.Length; i++)
            {
                if (!connections[i].IsCreated) continue;

                if (keepAliveStatusMap.ContainsKey(connections[i]))
                {
                    var status = keepAliveStatusMap[connections[i]];

                    var timeSinceLast = Time.time - status.lastSendTime;

                    // No need to send new message, we heard from the player less than five seconds ago.
                    if (timeSinceLast < KEEP_ALIVE_TIMEOUT) continue;

                    if (status.receivedReplySinceLast)
                    {
                        // Everything is normal, next ping
                        SendPing(connections[i]);
                    }
                }
            }
        }

        private void SendPing(NetworkConnection connection)
        {
            var status = keepAliveStatusMap[connection];
            
            SendUnicast(connection, new PingMessage());
            status.receivedReplySinceLast = false;
            status.lastSendTime = Time.time;
            
            Debug.Log("ping");
        }

        public void MarkKeepAlive(int connectionId)
        {
            // Convert connection native list to array because LINQ doesn't work with native lists
            var connections = this.connections.ToArray();
            
            if (!connections.Any(c => c.InternalId == connectionId))
            {
                Debug.LogError($"Tried to mark a non-existing connection for keep-alive. (id {connectionId})");
                return;
            }

            var connection = connections.First(c => c.InternalId == connectionId);

            if (!connection.IsCreated)
            {
                Debug.LogError($"Tried to mark an inactive connection for keep-alive. (id {connectionId})");
                return;
            }

            if (keepAliveStatusMap.ContainsKey(connection))
            {
                Debug.LogWarning($"Tried to mark an already-marked connection for keep-alive. (id {connectionId})");
            }

            keepAliveStatusMap[connection] = new KeepAliveStatus {lastSendTime = 0};
            SendPing(connection);
        }

        public void UnmarkKeepAlive(NetworkConnection connection)
        {
            if (!keepAliveStatusMap.Remove(connection))
            {
                Debug.LogWarning(
                    $"Tried to unmark a connection for keep-alive that wasn't being kept alive. (id {connection.InternalId})");
            }
        }

        public void DisconnectClient(NetworkConnection connection)
        {
            driver.Disconnect(connection);
        }

        // Static handler functions
        //  - Client handshake                  (DONE)
        //  - Client chat message               (DONE)
        //  - Client chat exit                  (DONE)
        //  - Input update

        // static void HandleClientHandshake(Server serv, NetworkConnection connection, MessageHeader header)
        // {
        //     var message = header as HandshakeMessage;
        //
        //     // Add to list
        //     serv.nameList.Add(connection, message.name);
        //     var msg = $"{message.name.ToString()} has joined the chat.";
        //     serv.chat.NewMessage(msg, ChatCanvas.joinColor);
        //
        //     ChatMessage chatMsg = new ChatMessage {
        //         messageType = MessageType.Join,
        //         message = msg
        //     };
        //
        //     // Send all clients the chat message
        //     serv.SendBroadcast(chatMsg);
        //
        //     // spawn a non-local, server player
        //     GameObject player;
        //     uint networkId = 0;
        //     if (serv.networkManager.SpawnWithId(NetworkSpawnObject.PLAYER, NetworkManager.NextNetworkID, out player))
        //     {
        //         // Get and setup player instance
        //         NetworkedPlayer playerInstance = player.GetComponent<NetworkedPlayer>();
        //         playerInstance.isServer = true;
        //         playerInstance.isLocal = false;
        //         networkId = playerInstance.networkId;
        //
        //         serv.playerInstances.Add(connection, playerInstance);
        //
        //         // Send spawn local player back to sender
        //         var responseMsg = new HandshakeResponseMessage {
        //             message = $"Welcome {message.name.ToString()}!",
        //             networkId = playerInstance.networkId
        //         };
        //
        //         serv.SendUnicast(connection, responseMsg);
        //     }
        //     else
        //     {
        //         Debug.LogError("Could not spawn player instance");
        //     }
        //
        //     // Send all existing players to this player
        //     foreach (KeyValuePair<NetworkConnection, NetworkedPlayer> pair in serv.playerInstances)
        //     {
        //         if (pair.Key == connection) continue;
        //
        //         SpawnMessage spawnMsg = new SpawnMessage {
        //             networkId = pair.Value.networkId,
        //             objectType = NetworkSpawnObject.PLAYER
        //         };
        //
        //         serv.SendUnicast(connection, spawnMsg);
        //     }
        //
        //     // Send creation of this player to all existing players
        //     if (networkId != 0)
        //     {
        //         SpawnMessage spawnMsg = new SpawnMessage {
        //             networkId = networkId,
        //             objectType = NetworkSpawnObject.PLAYER
        //         };
        //         serv.SendBroadcast(spawnMsg, connection);
        //     }
        //     else
        //     {
        //         Debug.LogError("Invalid network id for broadcasting creation");
        //     }
        // }
        //
        // static void HandleClientExit(Server serv, NetworkConnection connection, MessageHeader header)
        // {
        //     ChatQuitMessage quitMsg = header as ChatQuitMessage;
        //
        //     if (serv.nameList.ContainsKey(connection))
        //     {
        //         var msg = $"{serv.nameList[connection]} has left the chat.";
        //         serv.chat.NewMessage(msg, ChatCanvas.leaveColor);
        //
        //         // Clean up
        //         serv.nameList.Remove(connection);
        //         // if you join and quit quickly, might not be in this dict yet
        //         if (serv.pongDict.ContainsKey(connection))
        //         {
        //             serv.pongDict.Remove(connection);
        //         }
        //
        //         connection.Disconnect(serv.driver);
        //
        //         uint destroyId = serv.playerInstances[connection].networkId;
        //         Destroy(serv.playerInstances[connection].gameObject);
        //         serv.playerInstances.Remove(connection);
        //
        //         // Build messages
        //         ChatMessage chatMsg = new ChatMessage {
        //             message = msg,
        //             messageType = MessageType.Quit
        //         };
        //
        //         DestroyMessage destroyMsg = new DestroyMessage {
        //             networkId = destroyId
        //         };
        //
        //         // Send Messages to all other clients
        //         serv.SendBroadcast(chatMsg, connection);
        //         serv.SendBroadcast(destroyMsg, connection);
        //     }
        //     else
        //     {
        //         Debug.LogError("Received exit from unlisted connection");
        //     }
        // }

        private void HandleClientPong(NetworkConnection connection, MessageHeader header)
        {
            if (connection.IsCreated)
            {
                keepAliveStatusMap[connection].receivedReplySinceLast = true;
            }
        }
    }
}
