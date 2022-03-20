using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using PackedNetworking.Packets;
using PackedNetworking.Threading;
using PackedNetworking.Util;
using UnityEngine;
using static PackedNetworking.INetworkBehaviour;
using static PackedNetworking.NetworkBehaviour;

namespace PackedNetworking.Server
{
    internal class ServerBehaviour : INetworkBehaviour
    {
        private bool _isRunning;
        private readonly int _maxPlayers;
        private TcpListener _tcpListener;
        private UdpClient _udpListener;
        private readonly Dictionary<int, Client> _clients = new Dictionary<int, Client>();

        public event Action onClientConnect;
        public event Action<int> onClientHandshakeComplete;
        public event Action<int> onClientDisconnect;
        private readonly List<PacketHandler> _packetHandlers = new List<PacketHandler>();
        
        public ServerBehaviour(int maxPlayers = 16)
        {
            this._maxPlayers = Mathf.Max(maxPlayers, 0);
            for (int i = 1; i <= _maxPlayers; i++) 
                _clients.Add(i, new Client(i, this));
        }

        public bool IsSetup => _isRunning;

        public void Setup()
        {
            if (_isRunning)
            {
                NetworkingLogs.LogWarning("Cannot start the server twice!");
                return;
            }
            NetworkingLogs.LogInfo("STARTING SERVER");
            
            _tcpListener = new TcpListener(IPAddress.Any, NetworkSettings.Port);
            _tcpListener.Start();
            StartListeningForNewTcpClient();
            
            _udpListener = new UdpClient(NetworkSettings.Port);
            BeginReadUdp();
            
            NetworkingLogs.LogInfo("SERVER IS WAITING FOR CONNECTIONS");
            _isRunning = true;
        }

        public void SendTcpPacket(Packet packet)
        {
            if (!(packet is IServerSendable serverPacket))
            {
                NetworkingLogs.LogError($"Trying to send a packet from the server which is not a server packet. Packet: {packet.GetType().Name}");
                return;
            }

            foreach (var client in _clients.Values)
            {
                if (!serverPacket.IsTargetingAllClients && client.id != serverPacket.TargetClient) continue;
                if(!client.IsUsed) continue;
                
                if (serverPacket.IsTargetingAllClients)
                {
                    client.SendTcpData(packet, client.id);
                    packet.UndoBuild();
                }
                else
                {
                    client.SendTcpData(packet);
                    return;
                }
            }
        }

        public void SendUdpPacket(Packet packet)
        {
            if (!(packet is IServerSendable serverPacket))
            {
                NetworkingLogs.LogError($"Trying to send a packet from the server which is not a server packet. Packet: {packet.GetType().Name}");
                return;
            }

            foreach (var client in _clients.Values)
            {
                if (!serverPacket.IsTargetingAllClients && client.id != serverPacket.TargetClient) continue;
                if(!client.IsUsed) continue;
                
                if (serverPacket.IsTargetingAllClients)
                {
                    client.SendUdpData(packet, client.id);
                    packet.UndoBuild();
                }
                else
                {
                    client.SendUdpData(packet);
                    return;
                }
            }
        }

        public void ListenForPacket<T>(PacketHandler listener) where T : Packet
        {
            if(listener != null && !_packetHandlers.Contains(listener))
                _packetHandlers.Add(listener);
        }

        /// <summary>
        /// Callback called whenever a connection is incoming
        /// </summary>
        /// <param name="result"></param>
        void OnClientConnect(IAsyncResult result)
        {
            var connectingClient = _tcpListener.EndAcceptTcpClient(result);
            var client = GetNextUnusedClient();
            if (client == null)
            {
                NetworkingLogs.LogWarning("REJECTING CONNECTION: SERVER SEEMS TO BE FULL!");
                StartListeningForNewTcpClient();
                return;
            }
            
            client.StartRepresentingTcp(connectingClient);
            InvokeOnOnClientConnect();
            
            StartListeningForNewTcpClient();
        }

        void StartListeningForNewTcpClient()
        {
            if (_tcpListener == null) NetworkingLogs.LogFatal("Cannot start listening for new tcp clients, server has to be started first.");
            else _tcpListener.BeginAcceptTcpClient(OnClientConnect, null);
        }

        void BeginReadUdp()
        {
            _udpListener.BeginReceive(UdpReceiveCallback, null);
        }
        
        void UdpReceiveCallback(IAsyncResult result)
        {
            try
            {
                var clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var data = _udpListener.EndReceive(result, ref clientEndPoint);
                BeginReadUdp();
                
                if (data.Length < 4) return;

                using (var packet = new Packet(data))
                {
                    int clientId = packet.ReadInt();
                    if (clientId < 1) return;

                    if (_clients[clientId].udp.EndPoint == null)
                    {
                        _clients[clientId].udp.Connect(clientEndPoint);
                        return;
                    }

                    if (_clients[clientId].udp.EndPoint.ToString() == clientEndPoint.ToString())
                        _clients[clientId].udp.HandleData(packet);
                }
            }
            catch (Exception e)
            {
                NetworkingLogs.LogError($"Failed to receive Udp data: {e}");
            }
        }

        private void SendUdpData(IPEndPoint clientEndPoint, Packet packet)
        {
            try
            {
                if (clientEndPoint == null) return;
                _udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
            }
            catch (Exception e)
            {
                NetworkingLogs.LogError($"Failed to send data via Udp: {e}");
            }
        }
        
        /// <summary>
        /// Returns the next unused client in the client dictionary.
        /// </summary>
        /// <returns></returns>
        Client GetNextUnusedClient()
        {
            for (int i = 1; i < _maxPlayers; i++)
            {
                if (!_clients[i].IsUsed)
                    return _clients[i];
            }
            
            return null;
        }
        
        /// <summary>
        /// Returns the next unused client id.
        /// </summary>
        /// <returns></returns>
        public int GetNextHandshakeClientId()
        {
            for (int i = 1; i < _maxPlayers; i++)
            {
                if (!_clients[i].CompletedHandshake)
                    return i;
            }
            
            throw new Exception("Cannot get next unused client. The server seems to be full.");
        }
        
        public bool IsFull()
        {
            for (int i = 1; i < _maxPlayers; i++)
            {
                if (!_clients[i].IsUsed)
                    return false;
            }

            return true;
        }
        
        /// <summary>
        /// Represents a connected client.
        /// </summary>
        private class Client
        {
            public readonly int id;
            private readonly Tcp _tcp;
            public readonly Udp udp;
            private readonly ServerBehaviour _target;

            public Client(int id, ServerBehaviour target)
            {
                this.id = id;
                _target = target;
                _tcp = new Tcp(this);
                udp = new Udp(this);
            }

            /// <summary>
            /// Whether or not this object is already representing a client-socket.
            /// </summary>
            public bool IsUsed => _tcp.IsConnected;

            public bool IsFullySetUp => CompletedUdpTest && CompletedHandshake;
            public bool CompletedHandshake { get; set; }
            public bool CompletedUdpTest { get; set; }

            /// <summary>
            /// Refers this client to a socket.
            /// </summary>
            /// <param name="socket">The socket to represent.</param>
            public void StartRepresentingTcp(TcpClient socket)
            {
                if(socket == null) return;
                _tcp.Connect(socket);
            }

            /// <summary>
            /// Sends data to the client associated with this object using tcp.
            /// </summary>
            /// <param name="packet">The packet/data to send</param>
            /// <param name="overwrittenTargetClient">Adds</param>
            public void SendTcpData(Packet packet, int overwrittenTargetClient = -1)
            {
                if(packet == null) return;
                _tcp.SendData(packet, overwrittenTargetClient);
            }
            
            /// <summary>
            /// Sends data to the client associated with this object using udp.
            /// </summary>
            /// <param name="packet">The packet/data to send</param>
            /// <param name="overwrittenTargetClient">Adds</param>
            public void SendUdpData(Packet packet, int overwrittenTargetClient = -1)
            {
                if(packet == null) return;
                udp.SendData(packet, overwrittenTargetClient);
            }
            
            private void InvokePacketHandlers(Packet packet)
            {
                _target.InvokePacketHandlers(packet);
            }

            /// <summary>
            /// Represents the tcp connection.
            /// </summary>
            private class Tcp
            {
                private TcpClient _socket;

                private readonly Client _target;
                private NetworkStream _stream;
                private byte[] _receiveBuffer;
                private Packet _receivedData;

                /// <summary>
                /// Whether or not the connection has been established.
                /// </summary>
                public bool IsConnected => _socket != null;
                
                public Tcp(Client target)
                {
                    this._target = target;
                }

                /// <summary>
                /// Initializes and begins reading from the network stream.
                /// </summary>
                /// <param name="socket">The socket to connect to.</param>
                public void Connect(TcpClient socket)
                {
                    _socket = socket;
                    _socket.SendBufferSize = NetworkSettings.DataBufferSize;
                    _socket.ReceiveBufferSize = NetworkSettings.DataBufferSize;

                    _stream = _socket.GetStream();
                    
                    _receivedData = new Packet();
                    _receiveBuffer = new byte[NetworkSettings.DataBufferSize];

                    NetworkingLogs.LogInfo($"Connected new client! {GetClientSlotAmount()} more clients can join.");
                    
                    BeginRead();
                }

                /// <summary>
                /// Called when data has been received over the network stream.
                /// </summary>
                /// <param name="result"></param>
                void OnReceive(IAsyncResult result)
                {
                    try
                    {
                        var byteLength = _stream.EndRead(result);

                        if (byteLength <= 0)
                        {
                            DisconnectTcpAndUdp("Received byte-length was zero!");
                            return;
                        }
                        
                        var data = new byte[byteLength];
                        Array.Copy(_receiveBuffer, data, byteLength);
                        
                        _receivedData.Reset(HandleData(data));
                        BeginRead();
                    }
                    catch (Exception e)
                    {
                        DisconnectTcpAndUdp(e.ToString());
                    }
                }

                /// <summary>
                /// Begins an asynchronous read from the network stream.
                /// </summary>
                void BeginRead()
                {
                    _stream.BeginRead(_receiveBuffer, 0, NetworkSettings.DataBufferSize, OnReceive, null);
                }

                private bool HandleData(byte[] data)
                {
                    int packetLength = 0;

                    _receivedData.SetBytes(data);

                    if (_receivedData.UnreadLength() >= 4)
                    {
                        packetLength = _receivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }

                    while (packetLength > 0 && packetLength <= _receivedData.UnreadLength())
                    {
                        var packetBytes = _receivedData.ReadBytes(packetLength);
                        ThreadManager.ExecuteOnMainThread(() =>
                        {
                            using (var packet = new Packet(packetBytes))
                            {
                                _target.InvokePacketHandlers(packet);
                            }
                        });

                        packetLength = 0;
                        if (_receivedData.UnreadLength() >= 4)
                        {
                            packetLength = _receivedData.ReadInt();
                            if (packetLength <= 0)
                            {
                                return true;
                            }
                        }
                    }

                    return packetLength <= 1;
                }
                
                public void SendData(Packet data, int overwrittenTargetClient = -1)
                {
                    if(!IsConnected || data == null) return;
                    data.Build(overwrittenTargetClient);
                    if(data.Length() == 0) return;
                    
                    try
                    {
                        _stream.BeginWrite(data.ToArray(), 0, data.Length(),
                            null, null);
                    }
                    catch (Exception e)
                    {
                        NetworkingLogs.LogError($"Failed to send data to player {_target.id} via Tcp:\n{e}");
                    }
                }

                private void DisconnectTcpAndUdp(string logMessage)
                {
                    _target._target._clients[_target.id].Disconnect(logMessage);
                }
                
                public void Disconnect()
                {
                    _socket.Close();
                    _stream = null;
                    _receiveBuffer = null;
                    _receivedData = null;
                    _socket = null;
                    _target.CompletedHandshake = false;
                }

                private int GetClientSlotAmount()
                {
                    int unusedClientCount = 0;
                    
                    foreach (var keyValuePair in _target._target._clients)
                    {
                        var client = keyValuePair.Value;
                        if (!client.IsUsed) unusedClientCount++;
                    }
                    
                    return _target._target._maxPlayers - unusedClientCount;
                }
            }

            internal class Udp
            {
                public IPEndPoint EndPoint { get; private set; }
                private readonly Client _target;

                /// <summary>
                /// Whether or not the connection has been established.
                /// </summary>
                public bool IsConnected => EndPoint != null;
                
                public Udp(Client target)
                {
                    _target = target;
                }

                public void Connect(IPEndPoint endPoint)
                {
                    EndPoint = endPoint;
                    _target._target.SendUdpPacket(new UdpTest("Udp Test Message.", _target.id));
                }

                public void SendData(Packet data, int overwrittenTargetClient = -1)
                {
                    if(!IsConnected || data == null) return;
                    data.Build(overwrittenTargetClient);
                    if(data.Length() == 0) return;
                    
                    _target._target.SendUdpData(EndPoint, data);
                }

                internal void HandleData(Packet packet)
                {
                    int packetLength = packet.ReadInt();
                    var packetBytes = packet.ReadBytes(packetLength);

                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (var newPacket = new Packet(packetBytes))
                        {
                            _target.InvokePacketHandlers(newPacket);
                        }
                    });
                }
                
                private void DisconnectTcpAndUdp()
                {
                    _target._target._clients[_target.id].Disconnect();
                }
                
                public void Disconnect()
                {
                    EndPoint = null;
                    _target.CompletedUdpTest = false;
                }
            }
            
            private void Disconnect(string logMessage = "")
            {
                NetworkingLogs.LogInfo($"Disconnecting client with id '{id}'. {logMessage}");
                _tcp.Disconnect();
                udp.Disconnect();
                _target.InvokeOnOnClientDisconnect(id);
            }
        }

        private void InvokePacketHandlers(Packet packet)
        {
            var packetId = packet.ReadInt();
            
            if (!TryBuildPacketBy(packet, packetId, out var finalPacket))
            {
                NetworkingLogs.LogError($"Failed to build a packet with the id '{packetId}'. If this is the only error in the console, then it's not added to the supported packet list. Otherwise this error might be FATAL");
                return;
            }

            finalPacket.PacketId = packetId;
            
            foreach (var handler in _packetHandlers)
            {
                handler.Invoke(finalPacket);
            }
        }

        private void InvokeOnClientHandshakeComplete(int clientId)
        {
            onClientHandshakeComplete?.Invoke(clientId);
        }
        private void InvokeOnOnClientConnect()
        {
            ThreadManager.ExecuteOnNextUpdate(onClientConnect);
        }
        private void InvokeOnOnClientDisconnect(int clientId)
        {
            ThreadManager.ExecuteOnNextUpdate(delegate { onClientDisconnect?.Invoke(clientId); });
        }

        public void CompletedClientHandshake(int clientId)
        {
            _clients[clientId].CompletedHandshake = true;
            InvokeOnClientHandshakeComplete(clientId);
        }
        
        public void CompletedClientUdpTest(int clientId)
        {
            _clients[clientId].CompletedUdpTest = true;
        }
    }
}