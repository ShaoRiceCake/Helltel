using System.Collections.Generic;
using UnityEngine;

public class MonsterTriggerHandler : MonoBehaviour
{
    [Header("触发器检测设置")]
    [Tooltip("用于检测的触发器（如不指定则使用自身的触发器）")]
    public Collider triggerCollider;

    [Header("伤害系数")]
    public float damageCoefficient = 1f;

    private MRBunnyController bunnyController;
    private HashSet<IInteractable> activeInteractables = new HashSet<IInteractable>(); // 跟踪当前在触发器内的可交互对象

    private void Awake()
    {
        // 获取MRBunnyController组件
        bunnyController = GetComponent<MRBunnyController>();
        if (bunnyController == null)
        {
            bunnyController = GetComponentInParent<MRBunnyController>();
        }

        if (triggerCollider != null) return;
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            Debug.LogWarning("未找到触发器组件，请手动指定或添加触发器", this);
        }
        else
        {
            // 确保碰撞器是触发器
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable == null) return; // 如果没有接口，直接返回

        // 如果这个交互对象已经在触发器中，忽略
        if (activeInteractables.Contains(interactable)) return;

        // 添加到活动集合中
        activeInteractables.Add(interactable);

        // 计算最终伤害值
        var finalDamage = Mathf.RoundToInt(interactable.ItemDamage * damageCoefficient);
        
        // 调用MRBunnyController的受伤方法
        if (bunnyController != null)
        {
            bunnyController.TakeDamage(finalDamage);
        }

        var hitPosition = other.transform.position;
        
        Debug.Log("Trigger hit at position: " + hitPosition);
        
        // 出现受伤飘字    
        BroadcastMonsterHurtEvent(hitPosition, finalDamage, other);
        // 处理血液喷射效果
        HandleBloodSpray(hitPosition, finalDamage, other);
    }

    private void OnTriggerExit(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable == null) return;

        // 从活动集合中移除
        activeInteractables.Remove(interactable);
    }

    private void BroadcastMonsterHurtEvent(Vector3 hitPosition, float damage, Collider other)
    {
        var hurtEvent = new MonsterHurtEvent(
            hitPosition,
            damage,
            transform
        );
        EventBus<MonsterHurtEvent>.Publish(hurtEvent);
    }
    
    private void HandleBloodSpray(Vector3 hitPosition, float damage, Collider other)
    {
        var emitterCount = (int)damage;

        var otherRigidbody = other.attachedRigidbody;
        var speed = otherRigidbody != null ? otherRigidbody.velocity.magnitude : 0f;
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