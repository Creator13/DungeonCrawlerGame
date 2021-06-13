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
            {(ushort) DungenMessage.GameStartData, typeof(GameStartDataMessage)},
            {(ushort) DungenMessage.ClientReady, typeof(ClientReadyMessage)},
            {(ushort) DungenMessage.GameStarting, typeof(GameStartingMessage)},
            {(ushort) DungenMessage.PlayerJoined, typeof(PlayerJoinedMessage)},
            {(ushort) DungenMessage.PlayerLeft, typeof(PlayerLeftMessage)},
            {(ushort) DungenMessage.SetTurn, typeof(SetTurnMessage)},
            {(ushort) DungenMessage.MoveActionRequest, typeof(MoveActionRequestMessage)},
            {(ushort) DungenMessage.MoveActionPerformed, typeof(MoveActionPerformedMessage)},
        };
    }

    public enum DungenMessage : ushort
    {
        Handshake,
        HandshakeResponse,
        StartRequest,
        StartRequestResponse,
        GameStartData,
        ClientReady,
        GameStarting,
        PlayerJoined,
        PlayerLeft,
        SetTurn,
        MoveActionRequest, 
        MoveActionPerformed,
        Tick
        // World changes TODO
    }
}
