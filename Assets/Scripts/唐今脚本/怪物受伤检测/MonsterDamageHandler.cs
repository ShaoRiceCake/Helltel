using System.Linq;
using UnityEngine;

public class MonsterCollisionHandler : MonoBehaviour
{
    [Header("碰撞检测设置")]
    [Tooltip("用于检测碰撞的碰撞器（如不指定则使用自身的碰撞器）")]
    public Collider detectionCollider;

    [Header("伤害系数")]
    public float damageCoefficient = 1f;

    public MRBunnyController bunnyController;

    private bool _isMonsterDead;
    
    private void Awake()
    {
        // 获取MRBunnyController组件
        bunnyController = GetComponent<MRBunnyController>();
        if (bunnyController == null)
        {
            bunnyController = GetComponentInParent<MRBunnyController>();
        }

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

        // 计算最终伤害值
        var finalDamage = Mathf.RoundToInt(interactable.ItemDamage * damageCoefficient);
        
        if (bunnyController != null)
        {
            bunnyController.TakeDamage(finalDamage);
            _isMonsterDead = bunnyController.behaviorTree.Blackboard.Get<bool>("isDead");
        }

        var hitPosition = collision.transform.position;
        
        AudioManager.Instance.Play("兔子挨打", hitPosition);
        
        // 出现受伤飘字    
        BroadcastMonsterHurtEvent(hitPosition,finalDamage,collision);
        // 处理血液喷射效果
        HandleBloodSpray(hitPosition, finalDamage, collision);

        if (!_isMonsterDead) return;
        detectionCollider.enabled = false;
        this.enabled = false;
    }

    private void BroadcastMonsterHurtEvent(Vector3 hitPosition, float damage, Collision collision)
    {
        var hurtEvent = new MonsterHurtEvent(
            hitPosition,
            damage,
            transform
        );
        EventBus<MonsterHurtEvent>.Publish(hurtEvent);
    }

    private bool IsCollisionFromOurCollider(Collision collision)
    {
        return collision.contacts.Any(contact => contact.thisCollider == detectionCollider);
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

        // 发布血液喷射事件
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