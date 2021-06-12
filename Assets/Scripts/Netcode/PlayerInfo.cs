namespace Dungen.Netcode
{
    public struct PlayerInfo
    {
        public readonly uint networkId;
        public readonly string name;

        public PlayerInfo(uint networkId, string name)
        {
            this.networkId = networkId;
            this.name = name;
        }
    }
}
