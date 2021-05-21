using System;
using System.Collections.Generic;

namespace Dungen.Netcode
{
    public static class MessageInfo
    {
        public static Dictionary<ushort, Type> dungenTypeMap = new Dictionary<ushort, Type>{
            {(ushort) DungenMessages.Handshake, typeof(HandshakeMessage)},
            {(ushort) DungenMessages.HandshakeResponse, typeof(HandshakeResponseMessage)}
        };
    }
    
    public enum DungenMessages : ushort
    {
        Handshake, 
        HandshakeResponse
    }
}
