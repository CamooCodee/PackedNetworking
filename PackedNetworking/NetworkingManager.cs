using PackedNetworking.Threading;
using PackedNetworking.Util;
using UnityEngine;

namespace PackedNetworking
{
    [DefaultExecutionOrder(-1001)]
    public class NetworkingManager : MonoBehaviour
    {
        [Header("Settings:")]
        [SerializeField] private string serverIp = "127.0.0.1";
        [SerializeField] private int port = 9000;
        [SerializeField] private int maxClients = 4;
        [SerializeField] private bool connectOnApplicationStart = true;

        [Header("Server Detection:")]
        [SerializeField] private bool forceServerBuild;
        [SerializeField] private string serverSceneName;
        [SerializeField] private string clientSceneName;
        
        [Header("Automatic Packet Finder:")]
        [SerializeField, Tooltip("RECOMMENDED!!! Whether or not all packet classes should be found and added automatically.")]
        private bool active = true;
        [SerializeField, Tooltip("This will print every packet class that was found.")]
        private bool printPacketClasses;

        private void Awake()
        {
            NetworkBehaviour.connectOnApplicationStart = connectOnApplicationStart;
            NetworkBehaviour.IsServerBuild = Application.isBatchMode || forceServerBuild;

            if(!connectOnApplicationStart)
                return;
            
            Setup();
        }

        internal void Setup()
        {
            NetworkSettings.Port = port;
            NetworkSettings.ServerIp = serverIp;
            NetworkSettings.MaxPlayers = maxClients;
            
            gameObject.AddComponent<GameLifetimeGameObject>();
            
            var detector = gameObject.AddComponent<ServerDetector>();
            detector.SetValues(serverSceneName, clientSceneName);
            
            if (NetworkBehaviour.IsServerBuild)
                NetworkingLogs.Prefix = "[server] ";
            else
                NetworkingLogs.Prefix = "[client] ";
            
            var autoSupport = gameObject.AddComponent<AutoPacketSupporter>();
            autoSupport.SetValues(active, printPacketClasses);
            gameObject.AddComponent<ClientHandshake>();
            gameObject.AddComponent<ServerHandshake>();
            gameObject.AddComponent<ThreadManager>();
        }

        internal void SetIp(string newValue)
        {
            if(NetworkSettings.IsValidIpAddress(newValue))
                serverIp = newValue;
            else
                NetworkingLogs.LogError(
                    $"Trying to set an invalid ip-address '{newValue}'. Make sure it's in the correct format.");
        }
    }
}