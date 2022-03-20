using PackedNetworking.Client;
using PackedNetworking.Packets;
using PackedNetworking.Util;
using UnityEngine;

namespace PackedNetworking
{
    internal class ClientHandshake : ClientNetworkBehaviour
    {
        private bool _handshakeCompleted;
        private bool _udpTestCompleted;
        
        private void OnEnable()
        {
            NetworkingLogs.LogInfo("Listening For Handshake!");
            ListenForPacket<HandshakePacket>(OnHandshakeReceived);
            ListenForPacket<UdpTest>(OnUdpTestReceived);
        }

        void OnHandshakeReceived(Packet packet)
        {
            var handshake = (HandshakePacket) packet;
            NetworkingLogs.LogInfo("Received Handshake: " + handshake.message);
            ClientInstance.ClientId = handshake.clientId;
            SendTcpPacket(new HandshakeReceivedPacket(ClientId));
            ClientInstance.ConnectUdp();
            _handshakeCompleted = true;
            ClientInstance.onHandshakeReceived?.Invoke();
            if(_udpTestCompleted) Destroy(this);
        }

        void OnUdpTestReceived(Packet packet)
        {
            var udpTest = (UdpTest) packet;
            NetworkingLogs.LogInfo("Received Udp Test: " + udpTest.message);
            SendUdpPacket(new UdpTestReceived(ClientId));
            _udpTestCompleted = true;
            if(_handshakeCompleted) Destroy(this);
        }
    }
}