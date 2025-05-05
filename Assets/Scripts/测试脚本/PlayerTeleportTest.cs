using UnityEngine;
using System.Collections;
public class PlayerTeleportTest : MonoBehaviour
{
    [SerializeField] 
    private Transform _targetPosition; // 目标位置的Transform
    
    [SerializeField]
    private KeyCode _teleportKey = KeyCode.M; // 触发位移的按键
    
    [SerializeField]
    private float _teleportDelay = 0.5f; // 位移延迟时间（用于演示禁用效果）
    
    private void Update()
    {
        if (Input.GetKeyDown(_teleportKey))
        {
            if (_targetPosition == null)
            {
                Debug.LogError("目标位置未设置!", this);
                return;
            }
            
            StartCoroutine(TeleportWithDelay());
        }
    }
    
    private IEnumerator TeleportWithDelay()
    {
        Debug.Log("开始位移过程...");
        
        // 广播位移事件
        EventBus<PlayerTeleportEvent>.Publish(new PlayerTeleportEvent(_targetPosition.position));
        Debug.Log($"已广播位移事件，目标位置: {_targetPosition.position}");
        
        // 等待一段时间（演示用）
        yield return new WaitForSeconds(_teleportDelay);
        
        Debug.Log("位移过程完成");
    }
}