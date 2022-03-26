namespace PackedNetworking.Packets
{
    internal class HandshakeReceivedPacket : ClientPacket
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