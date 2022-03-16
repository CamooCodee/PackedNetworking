namespace PackedNetworking.Packets
{
    public class UdpTestReceived : ClientPacket
    {
        public const int ID = (int) PacketTypes.UdpTestReceived;
        
        public UdpTestReceived(int sendingClient) : base(ID, sendingClient)
        {
            
        }
        
        public UdpTestReceived(Packet packet) : base(ID, packet)
        {
               
        }
    }
}