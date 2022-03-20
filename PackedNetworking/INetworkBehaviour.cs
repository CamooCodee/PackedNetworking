using PackedNetworking.Packets;

namespace PackedNetworking
{
    public interface INetworkBehaviour
    {
        public bool IsSetup { get; }
        public delegate void PacketHandler(Packet packet);
        public void Setup();
        public void SendTcpPacket(Packet packet);
        public void SendUdpPacket(Packet packet);
        public void ListenForPacket<T>(PacketHandler listener) where T : Packet;
    }
}