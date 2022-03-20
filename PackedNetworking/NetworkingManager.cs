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
        
        [Header("Automatic Packet Finder:")]
        [SerializeField, Tooltip("RECOMMENDED!!! Whether or not all packet classes should be found and added automatically.")]
        private bool active = true;
        [SerializeField, Tooltip("This will print every packet class that was found.")]
        private bool printPacketClasses;

        [Header("Useful:")]
        [SerializeField] private bool makeBuildFullscreen = true;
        [SerializeField] private Vector2 windowSize = new Vector2(1920, 1080);
        
        private void Awake()
        {
            NetworkingLogs.Set(Debug.Log, Debug.LogWarning, Debug.LogError, Debug.LogError);
            
            if (!makeBuildFullscreen)
            {
                Screen.SetResolution((int) windowSize.x,
                    (int) windowSize.y,
                    FullScreenMode.Windowed);
            }

            NetworkBehaviour.connectOnApplicationStart = connectOnApplicationStart;
            NetworkSettings.Port = port;
            NetworkSettings.ServerIp = serverIp;
            NetworkSettings.MaxPlayers = maxClients;
            
            gameObject.AddComponent<GameLifetimeGameObject>();
            var detector = gameObject.AddComponent<ServerDetector>();
            detector.SetValues(forceServerBuild, serverSceneName);
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
    }
}