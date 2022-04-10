using PackedNetworking.Packets;
using PackedNetworking.Server;
using PackedNetworking.Util;

namespace PackedNetworking
{
    internal class ServerHandshake : ServerNetworkBehaviour
    {
        private void OnEnable()
        {
            ListenForPacket<HandshakeReceivedPacket>(OnCompletedHandshake);
            ListenForPacket<UdpTestReceived>(OnCompletedUdpTest);
        }

        public override void OnClientConnect()
        {
            int clientId = ServerInstance.GetNextHandshakeClientId();
            SendHandshake(clientId);
        }

        void SendHandshake(int clientId)
        {
            //NetworkingLogs.LogInfo($"Sending Handshake to {clientId}");
            var packet = new HandshakePacket($"Handshake Message. Your client ID: {clientId}", clientId);
            SendTcpPacket(packet);
        }

        void OnCompletedHandshake(Packet packet)
        {
            var handshake = (HandshakeReceivedPacket)packet;
            //NetworkingLogs.LogInfo($"Successfully connected to a new client! Id: {handshake.sendingClient}");
            ServerInstance.CompletedClientHandshake(handshake.sendingClient);
        }
        
        void OnCompletedUdpTest(Packet packet)
        {
            var udpTest = (UdpTestReceived)packet;
            NetworkingLogs.LogInfo($"Successfully connected new client! Id: {udpTest.sendingClient} | {ServerInstance.GetClientSlotAmount()} clients can join.");
            ServerInstance.CompletedClientUdpTest(udpTest.sendingClient);
        }
    }
}