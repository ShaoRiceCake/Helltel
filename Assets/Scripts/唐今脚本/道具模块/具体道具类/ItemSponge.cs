using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class ItemSponge : PassiveItem, ICleanable
{
    [Header("Cleaning Settings")]
    [SerializeField] private ObiColliderBase eraserCollider; 
    
    private void Start()
    {
        // 自动获取ObiCollider（如果没有手动指定）
        if (eraserCollider == null)
        {
            eraserCollider = GetComponent<ObiColliderBase>();
        }
        
        // 确保碰撞体已启用
        eraserCollider.enabled = true;
    }
    
    // 实现ICleanable接口
    public ObiColliderBase GetObiCollider()
    {
        return eraserCollider;
    }
    
    // 启用/禁用清洁功能
    public void EnableCleaning(bool enable)
    {
        if (eraserCollider != null)
        {
            eraserCollider.enabled = enable;
        }
    }
}
