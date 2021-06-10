﻿using System;
using System.Collections.Generic;

namespace Dungen.Netcode
{
    public static class MessageInfo
    {
        public static readonly Dictionary<ushort, Type> dungenTypeMap = new Dictionary<ushort, Type> {
            {(ushort) DungenMessage.Handshake, typeof(HandshakeMessage)},
            {(ushort) DungenMessage.HandshakeResponse, typeof(HandshakeResponseMessage)},
            {(ushort) DungenMessage.StartRequest, typeof(StartRequestMessage)},
            {(ushort) DungenMessage.StartRequestResponse, typeof(StartRequestResponseMessage)},
            {(ushort) DungenMessage.PlayerJoined, typeof(PlayerJoinedMessage)},
            {(ushort) DungenMessage.PlayerLeft, typeof(PlayerLeftMessage)},
        };
    }

    public enum DungenMessage : ushort
    {
        Handshake,
        HandshakeResponse,
        StartRequest,
        StartRequestResponse,
        GameStartData,
        PlayerReady,
        GameStarting,
        PlayerJoined,
        PlayerLeft,
        SetTurn,
        TurnAction, // TODO specificize

        // TurnActionConfirmed?
        TurnActionPerformed,
        Tick,
        // World changes TODO
    }
}
