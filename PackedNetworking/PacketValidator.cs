using PackedNetworking.Packets;

namespace PackedNetworking.Util
{
    internal static class PacketValidator
    {
        public static class Client
        {
            internal static readonly TypeValidator send =
                new TypeValidator(typeof(ClientPacket), typeof(ClientServerPacket));
            
            internal static readonly TypeValidator receive =
                new TypeValidator(typeof(ServerPacket), typeof(ClientServerPacket));
        }
        
        public static class Server
        {
            internal static readonly TypeValidator send =
                new TypeValidator(typeof(ServerPacket), typeof(ClientServerPacket));
            
            internal static readonly TypeValidator receive =
                new TypeValidator(typeof(ClientPacket), typeof(ClientServerPacket));
        }
    }
}