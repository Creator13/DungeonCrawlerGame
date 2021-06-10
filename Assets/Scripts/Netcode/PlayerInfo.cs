namespace Dungen.Netcode
{
    public struct PlayerInfo
    {
        public readonly uint playerId;
        public readonly string name;

        public PlayerInfo(uint playerId, string name)
        {
            this.playerId = playerId;
            this.name = name;
        }
    }
}
