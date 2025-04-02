using Unity.Netcode;
using UnityEngine;

public class NetworkLauncher : MonoBehaviour
{
    void Start()
    {
#if UNITY_EDITOR
        // 启动为 Host
        NetworkManager.Singleton.StartHost();
        Debug.Log("以 Host 模式启动");
#endif
    }
}