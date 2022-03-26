using PackedNetworking.Util;

namespace PackedNetworking.Packets
{
    /// <summary>
    /// Send from server to client.
    /// </summary>
    public abstract class ServerPacket : Packet, IServerSendable
    {
        private readonly int _targetClient;
        private readonly bool _isTargetingAllClients;
        
        /// <param name="packetId">The packet id defining what type of packet this is.</param>
        /// <param name="targetClient">The client id of the client targeted by this packet.</param>
        public ServerPacket(int packetId, int targetClient) : base(packetId)
        {
            if (targetClient < 0)
            {
                NetworkingLogs.LogError($"The target client can't be a negative value: '{targetClient}'");
                targetClient = 0;
            }
            this._targetClient = targetClient;
            _isTargetingAllClients = false;
        }
        
        /// <param name="packetId">The packet id defining what type of packet this is.</param>
        public ServerPacket(int packetId) : base(packetId)
        {
            _isTargetingAllClients = true;
            _targetClient = -1;
        }

        internal override void Build(int overwrittenTargetClient = -1)
        {
            if (_isTargetingAllClients && overwrittenTargetClient < 0)
            {
                NetworkingLogs.LogFatal($"Can't build '{GetType().Name}' packet. The target client has to be overwritten or set in the packet.");
                return;
            }

            var target = _targetClient;
            if (_isTargetingAllClients) target = overwrittenTargetClient;

            InsertInt(target);
            InsertInt(PacketId);
            base.Build(overwrittenTargetClient);
        }

        internal override void UndoBuild()
        {
            base.UndoBuild();
            RemoveLeadingInt();
            RemoveLeadingInt();
        }

        /// <summary>
        /// Whether or not the constructor for targeting all clients was used to instantiate this packet.
        /// </summary>
        public bool IsTargetingAllClients => _isTargetingAllClients;
        int IServerSendable.TargetClient => _targetClient;
        /// <summary>
        /// The client targeted by this packet.
        /// </summary>
        public int targetClient => _targetClient;
    }
}