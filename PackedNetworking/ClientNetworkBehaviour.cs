using System;
using System.Collections.Generic;
using PackedNetworking.Packets;
using PackedNetworking.Util;
using UnityEngine;
using static PackedNetworking.INetworkBehaviour;
using static PackedNetworking.PacketValidator.Client;

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

        public virtual void OnHandshakeReceived()
        {
            
        }
    }
}