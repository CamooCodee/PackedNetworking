using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using PackedNetworking.Packets;
using PackedNetworking.Threading;
using UnityEngine;
using static PackedNetworking.INetworkBehaviour;
using static PackedNetworking.NetworkBehaviour;

namespace PackedNetworking.Client
{
    public class ClientBehaviour : INetworkBehaviour
    {
        private Tcp _tcp;
        private Udp _upd;
        private readonly List<PacketHandler> _packetHandlers = new List<PacketHandler>();
        
        private int _clientId = -1;
        public int ClientId
        {
            get
            {
                if(_clientId < 0)
                    throw new Exception("The client id of this client hasn't been defined. Yet you are still trying to access it.");

                return _clientId;
            }
            set
            {
                if (_clientId > 0)
                {
                    Debug.LogWarning("The client id cannot be changed. Skipped this call.");
                    return;
                }
                if (value <= 0)
                {
                    Debug.LogError($"Trying to assign '{value}' as the client id. Client ids can only be positive! Skipped this call.");   
                    return;
                }

                _clientId = value;
            }
        }

        public bool IsConnected { get; private set; }

        public void Disconnect()
        {
            if (!IsConnected) return;
            
            IsConnected = false;
            _tcp.DisconnectSocket();
            _upd.DisconnectSocket();
            Debug.Log("Disconnected from server!");
        }

        public void Setup()
        {
            _tcp = new Tcp(this);
            IsConnected = true;
            _tcp.Connect();
        }
        
        public void SendTcpPacket(Packet packet)
        {
            packet.Build();
            _tcp.SendData(packet);
        }

        public void SendUdpPacket(Packet packet)
        {
            packet.Build();
            _upd.SendData(packet);
        }

        public void ListenForPacket<T>(PacketHandler listener) where T : Packet
        {
            if(listener != null && !_packetHandlers.Contains(listener))
                _packetHandlers.Add(listener);
        }

        internal void ConnectUdp()
        {
            _upd = new Udp(this);
            _upd.Connect(_tcp.LocalEndPoint.Port);
        }
        
        private bool _isHandshake = true;
        
        public void InvokePacketHandlers(Packet packet)
        {
            var packetId = packet.ReadInt();
            var targetClient = packet.ReadInt();

            bool isProperHandshake = _isHandshake && targetClient > 0;
            bool isLookingForPacket = isProperHandshake || targetClient == ClientId || targetClient < 0;
            
            if(!isLookingForPacket)
                throw new Exception("FATAL: Received a packet which wasn't aimed for this client.");

            _isHandshake = false;
            
            if (!TryBuildPacketBy(packet, packetId, out var finalPacket))
            {
                Debug.LogError($"Failed to build a packet with the id '{packetId}'. Make sure it's added to the supported packets if this is the only exception.");
                return;
            }

            finalPacket.PacketId = packetId;
            
            foreach (var handler in _packetHandlers)
            {
                handler.Invoke(finalPacket);
            }
        }
        
        private class Tcp
        {
            private TcpClient _socket;

            private NetworkStream _stream;
            private Packet _receivedData;
            private byte[] _receiveBuffer;
            private readonly ClientBehaviour _target;

            internal IPEndPoint LocalEndPoint => (IPEndPoint) _socket.Client.LocalEndPoint;

            public Tcp(ClientBehaviour target)
            {
                this._target = target;
            }
            
            public void Connect()
            {
                _socket = new TcpClient
                {
                    ReceiveBufferSize = NetworkSettings.DataBufferSize,
                    SendBufferSize = NetworkSettings.DataBufferSize
                };

                _receiveBuffer = new byte[NetworkSettings.DataBufferSize];
                _socket.BeginConnect(NetworkSettings.ServerIp, NetworkSettings.Port, OnConnect, _socket);
            }

            /// <summary>
            /// Disconnect the tcp and udp as well as reset all fields.
            /// </summary>
            private void Disconnect()
            {
                DisconnectTcpAndUdp();
                _stream = null;
                _receiveBuffer = null;
                _receivedData = null;
                _socket = null;
            }
            
            /// <summary>
            /// Disconnect tcp and udp.
            /// </summary>
            private void DisconnectTcpAndUdp()
            {
                _target.Disconnect();
            }
            
            /// <summary>
            /// Close the tcp socket.
            /// </summary>
            public void DisconnectSocket()
            {
                _socket.Close();
            }
            

            private void OnConnect(IAsyncResult result)
            {
                _socket.EndConnect(result);
                if (!_socket.Connected) return;

                _stream = _socket.GetStream();

                _receivedData = new Packet();
                
                BeginRead();
            }

            private void OnReceive(IAsyncResult result)
            {
                try
                {
                    var byteLength = _stream.EndRead(result);

                    if (byteLength <= 0)
                    {
                        DisconnectTcpAndUdp();
                    }
                        
                    var data = new byte[byteLength];
                    Array.Copy(_receiveBuffer, data, byteLength);
                    
                    _receivedData.Reset(HandleData(data));
                    BeginRead();
                }
                catch (Exception)
                {
                    Disconnect();
                }
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

            /// <summary>
            /// Begins an asynchronous read from the network stream.
            /// </summary>
            void BeginRead()
            {
                _stream.BeginRead(_receiveBuffer, 0, NetworkSettings.DataBufferSize, OnReceive, null);
            }
            
            public void SendData(Packet data)
            {
                try
                {
                    if(_socket == null) return;

                    _stream.BeginWrite(data.ToArray(), 0, data.Length(), null, null);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error sending data to the server via TCP: {e}");
                    throw;
                }
            }
        }
        
        private class Udp
        {
            private readonly ClientBehaviour _target;
            private UdpClient _socket;
            private IPEndPoint _endPoint;

            public Udp(ClientBehaviour target)
            {
                _target = target;
                _endPoint = new IPEndPoint(IPAddress.Parse(NetworkSettings.ServerIp), NetworkSettings.Port);
            }

            public void Connect(int localPort)
            {
                Debug.Log("Connecting Udp");
                _socket = new UdpClient(localPort);
                    
                _socket.Connect(_endPoint);
                BeginRead();

                using (var packet = new Packet())
                    SendData(packet);
            }

            public void SendData(Packet packet)
            {
                try
                {
                    packet.InsertInt(_target.ClientId);
                    Debug.Log("Sending UDP Packet. Inserted: " + _target.ClientId);
                    _socket?.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error sending data via UDP: {e}");
                }
            }
            
            private void OnReceive(IAsyncResult result)
            {
                try
                {
                    var data = _socket.EndReceive(result, ref _endPoint);
                    BeginRead();

                    if (data.Length < 4)
                    {
                        DisconnectTcpAndUdp();
                        return;
                    }

                    HandleData(data);
                }
                catch (Exception)
                {
                    Disconnect();
                    throw;
                }
            }

            private void HandleData(byte[] data)
            {
                using (var packet = new Packet(data))
                {
                    int packetLength = packet.ReadInt();
                    data = packet.ReadBytes(packetLength);
                }
                
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (var packet = new Packet(data))
                    {
                        _target.InvokePacketHandlers(packet);
                    }
                });
            }

            private void BeginRead()
            {
                _socket.BeginReceive(OnReceive, null);
            }

            /// <summary>
            /// Disconnect the tcp connection.
            /// </summary>
            private void Disconnect()
            {
                DisconnectTcpAndUdp();
                _socket = null;
                _endPoint = null;
            }
            
            /// <summary>
            /// Disconnect tcp and udp.
            /// </summary>
            private void DisconnectTcpAndUdp()
            {
                _target.Disconnect();
            }
            
            /// <summary>
            /// Close the tcp socket.
            /// </summary>
            public void DisconnectSocket()
            {
                _socket.Close();
            }
        }
    }
}