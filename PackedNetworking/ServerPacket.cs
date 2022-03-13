using PackedNetworking.Packets;
using UnityEngine;

namespace PackedNetworking.Server.Packets
{
    /// <summary>
    /// Send from server to client.
    /// </summary>
    public abstract class ServerPacket : Packet, IServerSendable
    {
        private readonly int _targetClient;
        private readonly bool _isTargetingAllClients;
        
        public ServerPacket(int packetId, int targetClient) : base(packetId)
        {
            if (targetClient < 0)
            {
                Debug.LogError($"The target client can't be a negative value: '{targetClient}'");
                targetClient = 0;
            }
            this._targetClient = targetClient;
            _isTargetingAllClients = false;
        }

        public ServerPacket(int packetId) : base(packetId)
        {
            _isTargetingAllClients = true;
            _targetClient = -1;
        }

        public override void Build(int overwrittenTargetClient = -1)
        {
            if (_isTargetingAllClients && overwrittenTargetClient < 0)
            {
                Debug.LogError($"Can't build '{GetType().Name}' packet. The target client has to be overwritten or set in the packet.");
                return;
            }

            var target = _targetClient;
            if (_isTargetingAllClients) target = overwrittenTargetClient;

            InsertInt(target);
            InsertInt(PacketId);
            base.Build(overwrittenTargetClient);
        }

        public override void UndoBuild()
        {
            base.UndoBuild();
            RemoveLeadingInt();
            RemoveLeadingInt();
        }

        public bool isTargetingAllClients => _isTargetingAllClients;
        int IServerSendable.targetClient => _targetClient;
        public int targetClient => _targetClient;
    }
}