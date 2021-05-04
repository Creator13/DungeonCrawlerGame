using Dungen.Netcode.Protocol;
using Unity.Networking.Transport;
using UnityEngine.Events;

namespace Networking.Protocol {
public class MessageEvent : UnityEvent<Message> { }

public abstract class Message {
    private static uint nextID = 0;
    private static uint NextID => ++nextID;

    public MessageType Type { get; protected set; } = MessageType.None;

    /// <summary>
    /// A new id is assigned to a message every time it is sent.
    /// </summary>
    public uint ID { get; private set; }

    protected virtual void SerializeObject(ref DataStreamWriter writer) {
        writer.WriteUShort((ushort) Type);
        writer.WriteUInt(ID);
    }

    protected virtual void DeserializeObject(ref DataStreamReader reader) {
        ID = reader.ReadUInt();
    }

    /// <summary>
    /// Reads and instantiates a message using the contents of a data stream. It expects the first part of the
    /// header of the message, which is the message type, to be read already. This value is passed on through the
    /// type parameter T.
    /// </summary>
    /// <param name="reader">A DataStreamReader which has the contents of the message type to be read, minus the
    /// message type part of the header.</param>
    /// <typeparam name="T">The type of the message to be read.</typeparam>
    /// <returns>The message read from the data stream as an object.</returns>
    public static T Read<T>(ref DataStreamReader reader) where T : Message, new() {
        var msg = new T();
        msg.DeserializeObject(ref reader);
        return msg;
    }

    /// <summary>
    /// Send a message instance over a writer stream. The message should be complete before being sent. This method
    /// generates a new ID for a message before sending.
    /// </summary>
    /// <param name="message">The message object to send.</param>
    /// <param name="writer">A DataStreamWriter to send the message to.</param>
    public static void Send(Message message, ref DataStreamWriter writer) {
        if (message.ID == 0) {
            message.ID = NextID;
        }

        message.SerializeObject(ref writer);
    }
}
}
