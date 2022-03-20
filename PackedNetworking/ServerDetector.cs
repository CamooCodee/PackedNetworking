using UnityEngine;
using UnityEngine.SceneManagement;
using static PackedNetworking.NetworkBehaviour;
using static UnityEngine.Application;

namespace PackedNetworking
{
    [DefaultExecutionOrder(-1000)]
    public class ServerDetector : MonoBehaviour
    {
        private string _serverScene;
        private string _clientScene;

        private void Start()
        {
            if(IsServerBuild && _serverScene.Length > 0)
                SceneManager.LoadScene(_serverScene);
            else if (!IsServerBuild && _clientScene.Length > 0)
                SceneManager.LoadScene(_clientScene);
            Destroy(this);
        }

        public void SetValues(string serverSceneName, string clientSceneName)
        {
            _serverScene = serverSceneName;
            _clientScene = clientSceneName;
        }
    }
}