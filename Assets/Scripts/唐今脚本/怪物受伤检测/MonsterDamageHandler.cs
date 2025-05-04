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
        if (detectionCollider != null) return;
        detectionCollider = GetComponent<Collider>();
        if (detectionCollider == null)
        {
            Debug.LogWarning("未找到碰撞器组件，请手动指定或添加碰撞器", this);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (detectionCollider != null && !IsCollisionFromOurCollider(collision))
        {
            return;
        }
        if (!collision.gameObject.CompareTag("Item")) return;
        var otherRigidbody = collision.gameObject.GetComponent<Rigidbody>();
        if (otherRigidbody == null) return;
        var mass = otherRigidbody.mass;
        var velocity = otherRigidbody.velocity;
        var speed = velocity.magnitude;
        
        if (speed < 10f) return;
        
        // 4. 计算动量 (动量 = 质量 * 速度)
        var momentum = mass * speed;
                
        var finalDamage = momentum * damageCoefficient;
                
        var hitPosition = collision.contacts.Length > 0 
            ? collision.contacts[0].point 
            : transform.position;
                
        // 7. 通过事件总线广播怪物受伤事件
        BroadcastMonsterHurtEvent(hitPosition, finalDamage, collision);
    }

    private bool IsCollisionFromOurCollider(Collision collision)
    {
        return collision.contacts.Any(contact => contact.thisCollider == detectionCollider);
    }

    // 广播怪物受伤事件
    private void BroadcastMonsterHurtEvent(Vector3 hitPosition, float damage, Collision collision)
    {
        var hurtEvent = new MonsterHurtEvent(
            monsterID,
            hitPosition,
            damage,
            transform 
        );
        
        // 通过事件总线发布事件
        EventBus<MonsterHurtEvent>.Publish(hurtEvent);
        
        HandleBloodSpray(hitPosition, damage, collision);
        
        // // 调试日志
        // Debug.Log($"怪物[{monsterID}]受伤！位置: {hitPosition}, 伤害: {damage}");
    }
    
    private void HandleBloodSpray(Vector3 hitPosition, float damage, Collision collision)
    {
        var emitterCount = Mathf.Min(1 + Mathf.FloorToInt(damage / 100f), 5);

        var otherRigidbody = collision.rigidbody;
        var speed = collision.relativeVelocity.magnitude;
        // float mass = otherRigidbody.mass;

        var emissionSpeed = Mathf.Lerp(1f, 10f, Mathf.InverseLerp(10f, 100f, speed));
        var randomness = Mathf.Lerp(0.3f, 0.8f, Mathf.InverseLerp(10f, 100f, speed));

        var direction = (hitPosition - transform.position).normalized;
        var rotation = Quaternion.LookRotation(direction);

        // 6. 发布血液喷射事件
        EventBus<BloodSprayEvent>.Publish(
            new BloodSprayEvent(
                hitPosition,
                rotation,
                emitterCount,
                emissionSpeed,
                randomness
            )
        );
    }

}