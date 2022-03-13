using PackedNetworking.Packets;

namespace PackedNetworking.Client.Packets
{
    public class HandshakeReceivedPacket : ClientPacket
    {
        public const int ID = (int) PacketTypes.HandshakeReceived;
        
        public HandshakeReceivedPacket(int sendingClient) : base(ID, sendingClient)
        {
            
        }
        
        public HandshakeReceivedPacket(Packet packet) : base(ID, packet)
        {
               
        }
    }
}