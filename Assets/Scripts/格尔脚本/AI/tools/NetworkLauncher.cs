using Unity.Netcode;
using UnityEngine;

public class NetworkLauncher : MonoBehaviour
{
    void Start()
    {
#if UNITY_EDITOR
        // ����Ϊ Host
        NetworkManager.Singleton.StartHost();
        Debug.Log("�� Host ģʽ����");
#endif
    }
}