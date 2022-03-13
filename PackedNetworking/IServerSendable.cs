namespace PackedNetworking
{
    internal interface IServerSendable
    {
        public bool isTargetingAllClients { get; }
        internal int targetClient { get; }
    }
}