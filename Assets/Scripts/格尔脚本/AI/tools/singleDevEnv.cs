using Unity.Netcode;
using UnityEngine;

public class singleDevEnv : MonoBehaviour
{

    void Start()
    {
        if (!NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.StartHost();  // 启动本地 Host 模式（包括 Server + Client）
        }
    }

}