using UnityEngine;

namespace PackedNetworking.General
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