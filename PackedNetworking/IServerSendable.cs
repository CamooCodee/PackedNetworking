namespace PackedNetworking.Packets
{
    internal interface IServerSendable
    {
        public bool IsTargetingAllClients { get; }
        internal int TargetClient { get; }
    }
}