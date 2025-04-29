using System.Linq;
using UnityEngine;

public class MonsterCollisionHandler : MonoBehaviour
{
    [Header("怪物设置")]
    [Tooltip("怪物ID（用于事件区分）")]
    public int monsterID = 0;

    [Header("碰撞检测设置")]
    [Tooltip("用于检测碰撞的碰撞器（如不指定则使用自身的碰撞器）")]
    public Collider detectionCollider;

    [Header("伤害计算")]
    [Tooltip("动量转化为伤害的系数")]
    [Range(0.1f, 10f)]
    public float damageCoefficient = 1f;

    private void Awake()
    {
        // 如果没有手动指定碰撞器，则使用自身的碰撞器
        if (detectionCollider != null) return;
        detectionCollider = GetComponent<Collider>();
        if (detectionCollider == null)
        {
            Debug.LogWarning("未找到碰撞器组件，请手动指定或添加碰撞器", this);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 检查是否是我们要检测的碰撞器发生的碰撞
        if (detectionCollider != null && !IsCollisionFromOurCollider(collision))
        {
            return;
        }

        // 1. 检查碰撞对象的tag是否为"Item"
        if (!collision.gameObject.CompareTag("Item")) return;
        // 2. 获取碰撞对象的质量
        var otherRigidbody = collision.gameObject.GetComponent<Rigidbody>();
        if (otherRigidbody == null) return;
        var mass = otherRigidbody.mass;
                
        // 3. 获取碰撞对象的速度
        var velocity = otherRigidbody.velocity;
        var speed = velocity.magnitude;
                
        // 4. 计算动量 (动量 = 质量 * 速度)
        var momentum = mass * speed;
                
        // 5. 应用伤害系数计算最终伤害值
        var finalDamage = momentum * damageCoefficient;
                
        // 6. 获取碰撞点位置（取第一个接触点的位置）
        var hitPosition = collision.contacts.Length > 0 
            ? collision.contacts[0].point 
            : transform.position;
                
        // 7. 通过事件总线广播怪物受伤事件
        BroadcastMonsterHurtEvent(hitPosition, finalDamage);
    }

    // 检查碰撞是否来自我们指定的碰撞器
    private bool IsCollisionFromOurCollider(Collision collision)
    {
        return collision.contacts.Any(contact => contact.thisCollider == detectionCollider);
    }

    // 广播怪物受伤事件
    private void BroadcastMonsterHurtEvent(Vector3 hitPosition, float damage)
    {
        // 创建事件对象（确保包含所有需要的信息）
        var hurtEvent = new MonsterHurtEvent(
            monsterID,
            hitPosition,
            damage,
            transform // 添加怪物transform引用，方便响应器获取更多信息
        );
        
        // 通过事件总线发布事件
        EventBus<MonsterHurtEvent>.Publish(hurtEvent);
        
        // 调试日志
        Debug.Log($"怪物[{monsterID}]受伤！位置: {hitPosition}, 伤害: {damage}");
    }
}