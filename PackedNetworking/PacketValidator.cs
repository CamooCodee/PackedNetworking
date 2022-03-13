using PackedNetworking.Client.Packets;
using PackedNetworking.Server.Packets;

namespace PackedNetworking
{
    public static class PacketValidator
    {
        public static class Client
        {
            public static readonly TypeValidator send =
                new TypeValidator(typeof(ClientPacket), typeof(ClientServerPacket));
            
            public static readonly TypeValidator receive =
                new TypeValidator(typeof(ServerPacket), typeof(ClientServerPacket));
        }
        
        public static class Server
        {
            public static readonly TypeValidator send =
                new TypeValidator(typeof(ServerPacket), typeof(ClientServerPacket));
            
            public static readonly TypeValidator receive =
                new TypeValidator(typeof(ClientPacket), typeof(ClientServerPacket));
        }
    }
}