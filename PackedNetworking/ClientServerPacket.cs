using PackedNetworking.Client;
using PackedNetworking.Util;

namespace PackedNetworking.Packets
{
    /// <summary>
    /// Send in both directions.
    /// </summary>
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
        /// <param name="packetId">The packet id defining what type of packet this is.</param>
        /// <param name="actingClient">The sending client id or the targeted client.</param>
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
        /// This will instantiate the packet as a server packet which is targeting all clients. Cannot be used on the client side.
        /// </summary>
        /// <param name="packetId">The packet id defining what type of packet this is.</param>
        public ClientServerPacket(int packetId) : base(packetId)
        {
            IsTargetingAllClients = true;
            _actingClient = -1;
        }
        
        /// <param name="packetId">The packet id defining what type of packet this is.</param>
        /// <param name="packet">A packet used to build up this packet type.</param>
        public ClientServerPacket(int packetId, Packet packet) : base(packetId)
        {
            if(NetworkBehaviour.IsServerBuild) _actingClient = packet.ReadInt();
        }

        internal override void Build(int overwrittenTargetClient = -1)
        {
            if (IsTargetingAllClients && overwrittenTargetClient < 0)
            {
                NetworkingLogs.LogError($"Can't build '{GetType().Name}' packet. The acting client has to be overwritten or set in the packet." +
                                        $" Make sure you are passing the client id when sending a '{nameof(ClientServerPacket)}' from the client.");
                return;
            }

            int acting = _actingClient;
            if (IsTargetingAllClients) acting = overwrittenTargetClient;
            
            InsertInt(acting);
            InsertInt(PacketId);
            base.Build(overwrittenTargetClient);
        }

        internal override void UndoBuild()
        {
            RemoveLeadingInt();
            RemoveLeadingInt();
            base.UndoBuild();
        }
    }
}