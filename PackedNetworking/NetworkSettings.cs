using System.Net;
using PackedNetworking.Util;
using UnityEngine;

namespace PackedNetworking
{
    internal static class NetworkSettings
    {
        private static int port = 9001;
        public static int Port
        {
            get => port;
            set => port = Mathf.Max(1, value);
        }
        
        private static int dataBufferSize = 4096;
        public static int DataBufferSize
        {
            get => dataBufferSize;
            set => Mathf.Max(2048, value);
        }

        private static int maxPlayers = 2;
        public static int MaxPlayers
        {
            get => maxPlayers;
            set => maxPlayers = Mathf.Max(1, value);
        }

        private static string serverIp = "127.0.0.1";

        public static string ServerIp
        {
            get => serverIp;
            set
            {
                var isValidId = IsValidIpAddress(value);
                if (!isValidId)
                    NetworkingLogs.LogWarning("Trying to set invalid server Ip. Value not changing.");
                else
                    serverIp = value;
            }
        }

        public static bool IsValidIpAddress(string ip) => IPAddress.TryParse(ip, out _);
    }
}