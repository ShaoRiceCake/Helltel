using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPosTool : MonoBehaviour
{
    // 静态变量存储位置
    public static Vector3 CurrentPosition { get; private set; }
    
    // 每个物理帧更新位置
    private void FixedUpdate()
    {
        CurrentPosition = transform.position;
    }

    // 静态方法获取位置 - 正确实现
    public static Vector3 GetTrackedPosition()
    {
        return CurrentPosition;
    }
}
