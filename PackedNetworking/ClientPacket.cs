namespace PackedNetworking.Packets
{
    /// <summary>
    /// Send from client to server.
    /// </summary>
    public abstract class ClientPacket : Packet
    {
        /// <summary>
        /// The client id of the client sending this packet.
        /// </summary>
        public readonly int sendingClient;
        
        /// <param name="packetId">The packet id defining what type of packet this is.</param>
        /// <param name="sendingClient">The client id of the client sending this packet.</param>
        public ClientPacket(int packetId, int sendingClient) : base(packetId)
        {
            this.sendingClient = sendingClient;
        }
        
        /// <param name="packetId">The packet id defining what type of packet this is.</param>
        /// <param name="packet">A packet used to build up this packet type.</param>
        public ClientPacket(int packetId, Packet packet) : base(packetId)
        {
            sendingClient = packet.ReadInt();
        }

        internal override void Build(int overwrittenTargetClient = -1)
        {
            InsertInt(sendingClient);
            InsertInt(PacketId);
            base.Build(overwrittenTargetClient);
        }

        internal override void UndoBuild()
        {
            base.UndoBuild();
            RemoveLeadingInt();
            RemoveLeadingInt();
        }
    }
}