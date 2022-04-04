using System;
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
        protected internal static event Action onSetup;

        /// <summary>
        /// If the current game instance is a server or a client.
        /// </summary>
        public static bool IsServerBuild { get; internal set; }

        internal static bool connectOnApplicationStart = false;

        protected internal static INetworkBehaviour behaviour;

        internal static ServerBehaviour ServerInstance
        {
            get
            {
                if (behaviour is ServerBehaviour sb) return sb;
                
                NetworkingLogs.LogFatal("FATAL: Trying to access the server instance on a client.");
                return null;
            }
        }
        protected static ClientBehaviour ClientInstance
        {
            get
            {
                if (behaviour is ClientBehaviour cb) return cb;
                
                NetworkingLogs.LogFatal("FATAL: Trying to access the client instance on the server.");
                return null;
            }
        }

        /// <summary>
        /// Every client has a unique id. This is the id of the client on the client side.
        /// </summary>
        public static int ClientId => ClientInstance.ClientId;
        
        protected internal static bool BehaviourIsSet => behaviour != null;

        private static readonly Dictionary<int, ConstructorInfo> packetConstructors = new Dictionary<int, ConstructorInfo>();

        /// <summary>
        /// Manually add a packet type that can then be received and send.
        /// </summary>
        /// <param name="id">The id of the packet to add.</param>
        /// <typeparam name="T">The type of the packet to add.</typeparam>
        public static void AddSupportedPacketType<T>(int id)
        {
            var type = typeof(T);
            AddSupportedPacketType(type, id);
        }

        /// <summary>
        /// Manually add a packet type that can then be received and send.
        /// </summary>
        /// <param name="type">The type of the packet to add.</param>
        /// <param name="id">The id of the packet to add.</param>
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

        internal static bool TryBuildPacketBy(Packet packet, int id, out Packet target)
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
            Setup(true, null);
            
            if (!BehaviourIsSet)
            {
                NetworkingLogs.LogWarning("There are Network-Behaviours but no Networking-Manager in your scene.");
            }
        }

        /// <summary>
        /// Boot client or server when 'connectOnApplicationStart' is set to false on the Networking Manager.
        /// </summary>
        /// <param name="manager">Your games Networking Manager</param>
        /// <param name="overwrittenIsServerBuild">Decides whether or not you want to start the server or client.</param>
        /// <param name="ip">An option to overwrite the server ip of the Networking Manager.</param>
        public static void BootNetworking(NetworkingManager manager, bool overwrittenIsServerBuild, string ip = null)
        {
            if (manager == null)
            {
                NetworkingLogs.LogError($"You cannot pass null to the '{nameof(NetworkingManager)}'!");
                return;
            }
            
            IsServerBuild = overwrittenIsServerBuild;
            if(ip != null)
                manager.SetIp(ip);
            Setup(false, manager);
        }
        /// <summary>
        /// Boot client or server when 'connectOnApplicationStart' is set to false on the Networking Manager.
        /// </summary>
        /// <param name="manager">Your games Networking Manager</param>
        /// <param name="ip">The server ip to connect to.</param>
        public static void BootNetworking(NetworkingManager manager, string ip = null)
        {
            if (manager == null)
            {
                NetworkingLogs.LogError($"You cannot pass null to the '{nameof(NetworkingManager)}'!");
                return;
            }
            if(ip != null)
                manager.SetIp(ip);
            Setup(false, manager);
        }
        
        /// <summary>
        /// Setup call only called when the behaviour is instantiated.
        /// </summary>
        private static void Setup(bool isApplicationStart, NetworkingManager manager)
        {
            if (BehaviourIsSet) return;
            if (!connectOnApplicationStart && isApplicationStart) return;

            if (IsServerBuild)
                behaviour = new ServerBehaviour(NetworkSettings.MaxPlayers);
            else
                behaviour = new ClientBehaviour();
            
            if(manager != null)
                manager.Setup();
            
            behaviour.Setup();
            onSetup?.Invoke();
        }
        
        protected internal void SendTcpPacket<PacketType>(PacketType packet) where PacketType : Packet
        {
            behaviour.SendTcpPacket(packet);
        }
        
        protected internal void SendUdpPacket<PacketType>(PacketType packet) where PacketType : Packet
        {
            behaviour.SendUdpPacket(packet);
        }
    }
}