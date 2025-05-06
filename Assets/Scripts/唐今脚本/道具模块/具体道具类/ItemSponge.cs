using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class ItemSponge : PassiveItem, ICleanable
{
    [Header("Cleaning Settings")]
    [SerializeField] private ObiColliderBase eraserCollider;
    
    protected override void Awake()
    {
        base.Awake();
        
        if (eraserCollider == null)
        {
            eraserCollider = GetComponent<ObiColliderBase>();
        }
        
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
