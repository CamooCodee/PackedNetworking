using UnityEngine;

namespace PackedNetworking.Util
{
    public class GameLifetimeGameObject : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Destroy(this);
        }
    }
}