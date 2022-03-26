namespace PackedNetworking.Packets
{
    internal class UdpTest : ServerPacket
    {
        public const int ID = (int) PacketTypes.UdpTest;
        public readonly string message;
        public readonly int clientId;

        public UdpTest(string message, int clientId) : base(ID, clientId)
        {
            this.message = message;
            this.clientId = clientId;
            
            Write(message);
            Write(clientId);
        }

        public UdpTest(Packet src) : base(ID)
        {
            message = src.ReadString();
            clientId = src.ReadInt();
        }
    }
}