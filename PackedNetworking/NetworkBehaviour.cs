﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PackedNetworking.Client;
using PackedNetworking.Packets;
using PackedNetworking.Server;
using PackedNetworking.Util;
using UnityEngine;

namespace PackedNetworking
{
    public abstract class NetworkBehaviour : MonoBehaviour
    {
        private static bool isServerBuildWasSet;
        private static bool isServerBuild;
        public static bool IsServerBuild
        {
            get => isServerBuild;
            set
            {
                if (isServerBuildWasSet)
                {
                    NetworkingLogs.LogError($"The '{nameof(IsServerBuild)}' property can only be set once. Skipped this call.");
                    return;
                }
                
                isServerBuildWasSet = true;
                isServerBuild = value;
            }
        }
        public static bool connectOnApplicationStart = false;

        protected static INetworkBehaviour behaviour;

        internal static ServerBehaviour ServerInstance
        {
            get
            {
                if (behaviour is ServerBehaviour sb) return sb;
                
                throw new Exception("FATAL: Trying to access the server instance on a client.");
            }
        }
        protected static ClientBehaviour ClientInstance
        {
            get
            {
                if (behaviour is ClientBehaviour cb) return cb;
                
                throw new Exception("FATAL: Trying to access the server instance on a client.");
            }
        }

        protected static int ClientId => ClientInstance.ClientId;
        
        private static bool IsSetUp => behaviour != null;

        protected static readonly Dictionary<int, ConstructorInfo> packetConstructors = new Dictionary<int, ConstructorInfo>();

        public static void AddSupportedPacketType<T>(int id)
        {
            var type = typeof(T);
            AddSupportedPacketType(type, id);
        }
        public static void AddSupportedPacketType(Type type, int id)
        {
            var constr = type.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 1 &&
                                                                    c.GetParameters().First().ParameterType == typeof(Packet));
            
            if(constr == null)
                NetworkingLogs.LogError($"Cannot support packet type '{type.Name}'. Packet types have to contain a constructor with a default packet as the parameter!");
            else if (!packetConstructors.ContainsKey(id))
                packetConstructors.Add(id, constr);
            else
                NetworkingLogs.LogWarning($"Already added a packet with id '{id}'. Not adding again.");
        }

        public static bool TryBuildPacketBy(Packet packet, int id, out Packet target)
        {
            try
            {
                target = (Packet)packetConstructors[id].Invoke(new object[] { packet });
            }
            catch (Exception e)
            {
                NetworkingLogs.LogError($"Failed to build packet with id: {id}: {e}");
                target = default;
                return false;
            }
            
            return true;
        }
        
        protected virtual void Awake()
        {
            if(!IsSetUp && (IsServerBuild || connectOnApplicationStart))
                Setup();
        }

        public static void ConnectToServer()
        {
            if (IsServerBuild)
            {
                NetworkingLogs.LogError($"You cannot call the '{nameof(ConnectToServer)}' method on the server!");
                return;
            }
            
            if(!IsSetUp)
                Setup();
            else
                NetworkingLogs.LogError("It seems like you've already set the client up! Cannot try to connect again.");
        }
        
        /// <summary>
        /// Setup call only called when the behaviour is instantiated.
        /// </summary>
        private static void Setup()
        {
            if(IsServerBuild)
                behaviour = new ServerBehaviour(NetworkSettings.MaxPlayers);
            else
                behaviour = new ClientBehaviour();

            behaviour.Setup();
        }
        
        protected void SendTcpPacket<PacketType>(PacketType packet) where PacketType : Packet
        {
            behaviour.SendTcpPacket(packet);
        }
        
        protected void SendUdpPacket<PacketType>(PacketType packet) where PacketType : Packet
        {
            behaviour.SendUdpPacket(packet);
        }
    }
}