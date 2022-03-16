using PackedNetworking.Packets;
using PackedNetworking.Server.Packets;

namespace Testing
{
    public class MyTestPacket : ServerPacket
    {
        public const int ID = 2;

        public readonly string someName;

        public MyTestPacket(string someName) : base(ID)
        {
            this.someName = someName;
            
            Write(this.someName);
        }

        public MyTestPacket(Packet packet) : base(ID)
        {
            someName = ReadString();
        }
    }
}