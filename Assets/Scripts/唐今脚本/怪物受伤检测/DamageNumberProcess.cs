using UnityEngine;
using DamageNumbersPro;
public class DamageNumberProcess : MonoBehaviour
{
    [Header("伤害数字设置")]
    public DamageNumber numberPrefab;
    
    [Header("显示设置")]
    [Tooltip("数字向上偏移量")]
    public float yOffset = 1f;
    [Tooltip("数字颜色渐变")]
    public Gradient colorGradient;
    [Tooltip("最小伤害值（颜色映射）")]
    public float minDamage = 0f;
    [Tooltip("最大伤害值（颜色映射）")]
    public float maxDamage = 500f;

    private void OnEnable()
    {
        EventBus<MonsterHurtEvent>.Subscribe(OnMonsterHurt, this);
    }

    private void OnDisable()
    {
        EventBus<MonsterHurtEvent>.UnsubscribeAll(this);
    }

    private void OnMonsterHurt(MonsterHurtEvent hurtEvent)
    {
        var displayPosition = hurtEvent.hurtPosition + Vector3.up * yOffset;
        var damageNumber = numberPrefab.Spawn(displayPosition, hurtEvent.damage);
        // damageNumber.SetScale(0.5f + hurtEvent.damage * 0.5f, 0);
    }
}
