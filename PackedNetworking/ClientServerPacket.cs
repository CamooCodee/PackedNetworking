using PackedNetworking.Client;
using PackedNetworking.Util;
using UnityEngine;

namespace PackedNetworking.Packets
{
    public abstract class ClientServerPacket : Packet, IServerSendable
    {
        /// <summary>
        /// Either the targeted client or the sending client.
        /// </summary>
        private readonly int _actingClient;

        public bool IsTargetingAllClients { get; }
        int IServerSendable.TargetClient => _actingClient;
        public int ActingClient
        {
            get
            {
                if(!NetworkBehaviour.IsServerBuild)
                    NetworkingLogs.LogWarning($"The '{nameof(ClientServerPacket)}.{nameof(ActingClient)}' property is always 0 on the client side. Use the ClientId of the {nameof(ClientNetworkBehaviour)} class instead!");
                return _actingClient;
            }
        }
        
        /// <summary>
        /// This will make the packet usable as a server or client packet.
        /// </summary>
        public ClientServerPacket(int packetId, int actingClient) : base(packetId)
        {
            if (actingClient < 0)
            {
                NetworkingLogs.LogError($"The target client can't be a negative value: '{actingClient}'");
                actingClient = 0;
            }
            this._actingClient = actingClient;
            IsTargetingAllClients = false;
        }

        /// <summary>
        /// This will instantiate the packet as a server packet which is targeting all clients.
        /// </summary>
        public ClientServerPacket(int packetId) : base(packetId)
        {
            IsTargetingAllClients = true;
            _actingClient = -1;
        }
        
        public ClientServerPacket(int packetId, Packet packet) : base(packetId)
        {
            if(NetworkBehaviour.IsServerBuild) _actingClient = packet.ReadInt();
        }
        
        public override void Build(int overwrittenTargetClient = -1)
        {
            if (IsTargetingAllClients && overwrittenTargetClient < 0)
            {
                NetworkingLogs.LogError($"Can't build '{GetType().Name}' packet. The target client has to be overwritten or set in the packet.");
                return;
            }

            int acting = _actingClient;
            if (IsTargetingAllClients) acting = overwrittenTargetClient;
            
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
    }
}