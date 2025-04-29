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
    public float maxDamage = 100f;

    private void OnEnable()
    {
        // 订阅怪物受伤事件
        EventBus<MonsterHurtEvent>.Subscribe(OnMonsterHurt, this);
    }

    private void OnDisable()
    {
        // 取消订阅
        EventBus<MonsterHurtEvent>.UnsubscribeAll(this);
    }

    private void OnMonsterHurt(MonsterHurtEvent hurtEvent)
    {
        // 计算显示位置（受伤位置 + 垂直偏移）
        var displayPosition = hurtEvent.hurtPosition + Vector3.up * yOffset;
        
        // 生成伤害数字
        var damageNumber = numberPrefab.Spawn(displayPosition, hurtEvent.damage);
        //
        // // 可选：添加更多视觉效果
        // damageNumber.enableLerp = true;
        //
        // // 根据伤害大小调整尺寸
        // damageNumber.SetScale(0.5f + hurtEvent.damage * 0.5f, 0);
    }
}
