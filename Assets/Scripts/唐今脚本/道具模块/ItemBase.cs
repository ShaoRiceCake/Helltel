using UnityEngine;

/// <summary>
/// 道具基类
/// </summary>
public abstract class ItemBase : MonoBehaviour, IPersistable
{
    [Header("基础道具设置")]
    [SerializeField] protected string itemName = "未命名道具";
    [SerializeField] protected Sprite itemIcon;
    [SerializeField] protected string itemDescription = "道具描述";

    public virtual void UseItem()
    {
        
    }

    public virtual void DropItem()
    {
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 获取道具信息
    /// </summary>
    public virtual string GetItemInfo()
    {
        return $"名称: {itemName}\n描述: {itemDescription}";
    }
}
