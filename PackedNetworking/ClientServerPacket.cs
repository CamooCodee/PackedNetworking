using PackedNetworking.Client;
using PackedNetworking.Packets;
using UnityEngine;

namespace PackedNetworking
{
    public abstract class ClientServerPacket : Packet, IServerSendable
    {
        /// <summary>
        /// Either the targeted client or the sending client.
        /// </summary>
        private readonly int _actingClient;
        private readonly bool _isTargetingAllClients;

        /// <summary>
        /// This will make the packet usable as a server or client packet.
        /// </summary>
        public ClientServerPacket(int packetId, int actingClient) : base(packetId)
        {
            if (actingClient < 0)
            {
                Debug.LogError($"The target client can't be a negative value: '{actingClient}'");
                actingClient = 0;
            }
            this._actingClient = actingClient;
            _isTargetingAllClients = false;
        }

        /// <summary>
        /// This will instantiate the packet as a server packet which is targeting all clients.
        /// </summary>
        public ClientServerPacket(int packetId) : base(packetId)
        {
            _isTargetingAllClients = true;
            _actingClient = -1;
        }
        
        public ClientServerPacket(int packetId, Packet packet) : base(packetId)
        {
            if(NetworkBehaviour.isServerBuild) _actingClient = packet.ReadInt();
        }
        
        public override void Build(int overwrittenTargetClient = -1)
        {
            if (_isTargetingAllClients && overwrittenTargetClient < 0)
            {
                Debug.LogError($"Can't build '{GetType().Name}' packet. The target client has to be overwritten or set in the packet.");
                return;
            }

            int acting = _actingClient;
            if (_isTargetingAllClients) acting = overwrittenTargetClient;
            
            InsertInt(acting);
            InsertInt(PacketId);
            base.Build(overwrittenTargetClient);
        }

        public override void UndoBuild()
        {
            RemoveLeadingInt();
            RemoveLeadingInt();
            base.UndoBuild();
        }

        public bool isTargetingAllClients => _isTargetingAllClients;
        int IServerSendable.targetClient => _actingClient;
        public int actingClient
        {
            get
            {
                if(!NetworkBehaviour.isServerBuild)
                    Debug.LogWarning($"The {nameof(actingClient)} property is always 0 on the client side. Use the ClientId of the {nameof(ClientNetworkBehaviour)} class instead!");
                return _actingClient;
            }
        }
    }
}