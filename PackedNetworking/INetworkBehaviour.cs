using PackedNetworking.Packets;

namespace PackedNetworking
{
    public interface INetworkBehaviour
    {
        /// <summary>
        /// This is a delegate for methods that are listening for packets on both, server and client side.
        /// </summary>
        public delegate void PacketHandler(Packet packet);
        internal void Setup();
        internal void SendTcpPacket(Packet packet);
        internal void SendUdpPacket(Packet packet);
        internal void ListenForPacket<T>(PacketHandler listener) where T : Packet;
    }
}