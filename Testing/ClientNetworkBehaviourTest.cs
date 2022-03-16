using System;
using PackedNetworking.Client;

namespace Testing
{
    public class ClientNetworkBehaviourTest : ClientNetworkBehaviour
    {
        private void Start()
        {
            ListenForPacket<MyTestPacket>(OnMyTestPacket);
        }

        private void OnMyTestPacket(MyTestPacket packet)
        {
            Console.WriteLine(packet.someName);
        }
    }
}