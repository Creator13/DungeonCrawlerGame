namespace Dungen.Netcode.Protocol {
public enum MessageType {
    Unresolved = -1,

    /// <summary>
    /// Empty. Multi-purpose.
    /// </summary>
    None = 0,

    /// <summary>
    /// Server -> client <br/>
    /// Sent to client as confirmation that they successfully joined the game. <br/>
    /// [(empty)]
    /// </summary>
    GameJoined = 1,

    /// <summary>
    /// Server -> all clients <br/>
    /// Sent to clients in game when a player leaves, to notify them a player left. <br/>
    /// [(empty)]
    /// </summary>
    PlayerLeft = 2,
    JoinRefused = 3,
    StartGame,
    TargetSpawned,
    RingCreated,
    RingReleased,
    GameWon,
    GameLost
}
}
