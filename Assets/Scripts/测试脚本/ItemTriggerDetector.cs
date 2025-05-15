using UnityEngine;

public class ItemTriggerDetector : MonoBehaviour
{
    // 当其他碰撞体进入触发器时调用
    private void OnTriggerEnter(Collider other)
    {
        // 检查进入的对象是否带有"Item"标签
        if (other.CompareTag("Item"))
        {
            Debug.Log("检测到Item对象进入触发器: " + other.gameObject.name);
        }
    }
}