using System;
using System.Collections.Generic;
using PackedNetworking.Packets;
using PackedNetworking.Util;
using static PackedNetworking.INetworkBehaviour;
using static PackedNetworking.Util.PacketValidator.Server;

namespace PackedNetworking.Server
{
    public abstract class ServerNetworkBehaviour : NetworkBehaviour
    {
        private readonly Dictionary<Type, List<PacketHandler>> _packetListeners = new Dictionary<Type, List<PacketHandler>>();
        
        /// <summary>
        /// Returns all ids of the currently connected clients.
        /// Clients don't have to be fully connected and set up in order to be included.
        /// </summary>
        protected int[] ConnectedClientIds => ServerInstance.GetAllConnectedClientIds();

        protected override void Awake()
        {
            if(BehaviourIsSet)
                Setup();
            else
                onSetup += Setup;
        }

        private void Setup()
        {
            if (!IsServerBuild)
            {
                Destroy(this);
                return;
            }

            base.Awake();

            if (!BehaviourIsSet)
                return;
            
            behaviour.ListenForPacket<Packet>(OnPacket);
            ServerInstance.onClientConnect += OnClientConnect;
            ServerInstance.onClientDisconnect += OnClientDisconnect;
            ServerInstance.onClientConnectionComplete += OnClientConnectionCompleted;
        }

        /// <summary>
        /// Sends the given packet the targeted client(s) using tcp.
        /// </summary>
        /// <param name="packet">The packet you want to send.</param>
        protected new void SendTcpPacket<PacketType>(PacketType packet) where PacketType : Packet
        {
            var type = typeof(PacketType);
            if (!send.IsValid(type))
            {
                NetworkingLogs.LogError($"You can only send {send.GetValidTypesAsString()} packets from a {nameof(ServerNetworkBehaviour)}!");
                return;
            }
            base.SendTcpPacket(packet);
        }
        /// <summary>
        /// Sends the given packet the targeted client(s) using udp.
        /// </summary>
        /// <param name="packet">The packet you want to send.</param>
        protected new void SendUdpPacket<PacketType>(PacketType packet) where PacketType : Packet
        {
            var type = typeof(PacketType);
            if (!send.IsValid(type))
            {
                NetworkingLogs.LogError($"You can only send {send.GetValidTypesAsString()} packets from a {nameof(ServerNetworkBehaviour)}!");
                return;
            }
            base.SendUdpPacket(packet);
        }

        /// <summary>
        /// Add a method for a packet you want to listen for.
        /// </summary>
        /// <param name="handler">The method executed when receiving the packet.</param>
        /// <typeparam name="PacketType">The type of the packet to listen for.</typeparam>
        protected void ListenForPacket<PacketType>(PacketHandler handler) where PacketType : Packet
        {
            var type = typeof(PacketType);
            if (!receive.IsValid(type))
            {
                NetworkingLogs.LogError($"You can only listen for {send.GetValidTypesAsString()} packets on a {nameof(ServerNetworkBehaviour)}!");
                return;
            }
            if(handler == null) return;
            
            var clientHandler = handler;
            if (_packetListeners.ContainsKey(type))
            {
                if(!_packetListeners[type].Contains(clientHandler))
                    _packetListeners[type].Add(clientHandler);
            }
            else
                _packetListeners.Add(type, new List<PacketHandler> {clientHandler});
        }

        /// <summary>
        /// Remove a method which is currently listening for a packet.
        /// </summary>
        /// <param name="handler">The method supposed to stop listening.</param>
        /// <typeparam name="PacketType">The type of the packet you want to stop listening for.</typeparam>
        protected void StopListeningForPacket<PacketType>(PacketHandler handler) where PacketType : Packet
        {
            if(handler == null) return;
            
            var type = typeof(PacketType);
            if (!_packetListeners.ContainsKey(type)) return;
            
            if(_packetListeners[type].Contains(handler))
                _packetListeners[type].Remove(handler);
        }

        void OnPacket(Packet packet)
        {
            var type = packet.GetType();
            if (!_packetListeners.ContainsKey(type))
                return;

            var toInvoke = _packetListeners[type];

            foreach (var listener in toInvoke) 
                listener.Invoke(packet);
        }

        /// <summary>
        /// Executed when a client connection is detected. DON'T send packets from inside of this method.
        /// </summary>
        public virtual void OnClientConnect()
        {
            
        }
        
        /// <summary>
        /// Executed when a client is fully connected to the server. You can send packets from inside of this method.
        /// </summary>
        /// <param name="clientId">The client id of the client connecting.</param>
        public virtual void OnClientConnectionCompleted(int clientId)
        {
            
        }
        
        /// <summary>
        /// Executed whenever a client disconnects.
        /// </summary>
        /// <param name="clientId">The client id of the client disconnecting.</param>
        public virtual void OnClientDisconnect(int clientId)
        {
            
        }
    }
}