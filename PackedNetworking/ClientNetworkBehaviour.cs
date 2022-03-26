using System;
using System.Collections.Generic;
using PackedNetworking.Packets;
using PackedNetworking.Util;
using static PackedNetworking.INetworkBehaviour;
using static PackedNetworking.Util.PacketValidator.Client;

namespace PackedNetworking.Client
{
    public class ClientNetworkBehaviour : NetworkBehaviour
    {
        private readonly Dictionary<Type, List<PacketHandler>> _packetListeners = new Dictionary<Type, List<PacketHandler>>();

        protected override void Awake()
        {
            if(BehaviourIsSet)
                Setup();
            else
                onSetup += Setup;
        }

        private void Setup()
        {
            if (IsServerBuild)
            {
                Destroy(this);
                return;
            }
            
            base.Awake();

            if (!BehaviourIsSet)
                return;

            ClientInstance.onHandshakeReceived += OnHandshakeReceived;
            behaviour.ListenForPacket<Packet>(OnPacket);
        }
        
        /// <summary>
        /// Sends the given packet to the server using tcp.
        /// </summary>
        /// <param name="packet">The packet you want to send.</param>
        protected new void SendTcpPacket<PacketType>(PacketType packet) where PacketType : Packet
        {
            var type = typeof(PacketType);
            if (!send.IsValid(type))
            {
                NetworkingLogs.LogError($"You can only send {send.GetValidTypesAsString()} packets from a {nameof(ClientNetworkBehaviour)}!");
                return;
            }
            
            base.SendTcpPacket(packet);
        }
        /// <summary>
        /// Sends the given packet to the server using udp.
        /// </summary>
        /// <param name="packet">The packet you want to send.</param>
        protected new void SendUdpPacket<PacketType>(PacketType packet) where PacketType : Packet
        {
            var type = typeof(PacketType);
            if (!send.IsValid(type))
            {
                NetworkingLogs.LogError($"You can only send {send.GetValidTypesAsString()} packets from a {nameof(ClientNetworkBehaviour)}!");
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
                NetworkingLogs.LogError($"You can only listen for {receive.GetValidTypesAsString()} packets on a {nameof(ClientNetworkBehaviour)}!");
                return;
            }
            
            if(handler == null) return;

            if (_packetListeners.ContainsKey(type))
            {
                if(!_packetListeners[type].Contains(handler))
                    _packetListeners[type].Add(handler);
            }
            else
            {
                _packetListeners.Add(type, new List<PacketHandler> { handler });
            }
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
        /// Executed when the client receives the handshake from the server.
        /// </summary>
        public virtual void OnHandshakeReceived()
        {
            
        }
    }
}