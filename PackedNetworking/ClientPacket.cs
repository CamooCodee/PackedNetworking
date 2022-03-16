namespace PackedNetworking.Packets
{
    /// <summary>
    /// Send from client to server.
    /// </summary>
    public abstract class ClientPacket : Packet
    {
        public readonly int sendingClient;
        
        public ClientPacket(int packetId, int sendingClient) : base(packetId)
        {
            this.sendingClient = sendingClient;
        }
        
        public ClientPacket(int packetId, Packet packet) : base(packetId)
        {
            sendingClient = packet.ReadInt();
        }

        public override void Build(int overwrittenTargetClient = -1)
        {
            InsertInt(sendingClient);
            InsertInt(PacketId);
            base.Build(overwrittenTargetClient);
        }

        public override void UndoBuild()
        {
            base.UndoBuild();
            RemoveLeadingInt();
            RemoveLeadingInt();
        }
    }
}