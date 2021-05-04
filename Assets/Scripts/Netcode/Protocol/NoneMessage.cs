using Networking.Protocol;

namespace Dungen.Netcode.Protocol {
public class NoneMessage : Message {
    public NoneMessage() {
        Type = MessageType.None;
    }
}
}
