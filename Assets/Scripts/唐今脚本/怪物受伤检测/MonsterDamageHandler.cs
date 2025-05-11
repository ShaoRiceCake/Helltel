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
        var interactable = collision.gameObject.GetComponent<IInteractable>();
        if (interactable == null) return; // 如果没有接口，直接返回

        var finalDamage = interactable.ItemDamage * damageCoefficient;

        var hitPosition = collision.contacts.Length > 0 
            ? collision.contacts[0].point 
            : transform.position;
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
        EventBus<MonsterHurtEvent>.Publish(hurtEvent);
        HandleBloodSpray(hitPosition, damage, collision);
        
    }
    
    private void HandleBloodSpray(Vector3 hitPosition, float damage, Collision collision)
    {
        var emitterCount = (int)damage;

        var otherRigidbody = collision.rigidbody;
        var speed = collision.relativeVelocity.magnitude;
        var emissionSpeed = Mathf.Lerp(1f, 10f, Mathf.InverseLerp(10f, 100f, speed));
        var randomness = 0.2f;

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