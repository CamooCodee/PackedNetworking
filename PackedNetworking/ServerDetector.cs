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

        private void Start()
        {
            if(IsServerBuild && _serverScene.Length > 0)
                SceneManager.LoadScene(_serverScene);
            Destroy(this);
        }

        public void SetValues(bool force, string serverSceneName)
        {
            _serverScene = serverSceneName;
            IsServerBuild = isBatchMode || force;
        }
    }
}