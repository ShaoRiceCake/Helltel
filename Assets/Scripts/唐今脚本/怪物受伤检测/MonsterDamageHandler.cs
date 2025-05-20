using System.Linq;
using UnityEngine;

public class MonsterCollisionHandler : MonoBehaviour
{
    [Header("碰撞检测设置")]
    [Tooltip("用于检测碰撞的碰撞器（如不指定则使用自身的碰撞器）")]
    public Collider detectionCollider;

    [Header("伤害系数")]
    public float damageCoefficient = 1f;
    
    private bool _isMonsterDead;
    
    public MRBunnyController bunnyController;
    private void Awake()
    {

        if (detectionCollider) return;
        detectionCollider = GetComponent<Collider>();
        if (!detectionCollider)
        {
            Debug.LogWarning("未找到碰撞器组件，请手动指定或添加碰撞器", this);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (detectionCollider && !IsCollisionFromOurCollider(collision))
        {
            return;
        }
        
        var interactable = collision.gameObject.GetComponent<IInteractable>();
        if (interactable == null) return; 

        var finalDamage = Mathf.RoundToInt(interactable.ItemDamage * damageCoefficient);

        this.gameObject.GetComponentInParent<IHurtable>()?.TakeDamage(finalDamage);

        if (collision.gameObject.GetComponentInParent<IDie>() is { } die)
        {
            _isMonsterDead = die.IsDead;
        }
        
        var hitPosition = collision.transform.position;
        
        // AudioManager.Instance.Play("兔子挨打", hitPosition);
        
        BroadcastMonsterHurtEvent(hitPosition,finalDamage,collision);

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