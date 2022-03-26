
namespace PackedNetworking.Packets
{
    internal class HandshakePacket : ServerPacket
    {
        public const int ID = (int) PacketTypes.Handshake;
        public readonly string message;
        public readonly int clientId;

        public HandshakePacket(string message, int clientId) : base(ID, clientId)
        {
            this.message = message;
            this.clientId = clientId;
            
            Write(message);
            Write(clientId);
        }

        public HandshakePacket(Packet src) : base(ID)
        {
            message = src.ReadString();
            clientId = src.ReadInt();
        }
    }
}