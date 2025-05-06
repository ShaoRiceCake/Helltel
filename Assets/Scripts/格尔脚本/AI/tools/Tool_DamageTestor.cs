using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class HurtTargetBinding
{
    public KeyCode triggerKey;
    public GameObject target;
    public int damage = 10;
}


public class Tool_DamageTestor : MonoBehaviour
{
    
    [Header("按键与伤害目标绑定")]
    public List<HurtTargetBinding> bindings = new List<HurtTargetBinding>();

    void Update()
    {
        foreach (var binding in bindings)
        {
            if (Input.GetKeyDown(binding.triggerKey))
            {
                TryHurt(binding);
            }
        }
    }

    private void TryHurt(HurtTargetBinding binding)
    {
        if (binding.target == null)
        {
            Debug.LogWarning($"❗按键 [{binding.triggerKey}] 没有设置目标！");
            return;
        }
        // bingding.target 在实际使用中，想办法获取到这个目标的引用，find，碰撞，或者显示引用等均可
        IHurtable hurtable = binding.target.GetComponent<IHurtable>();
        if (hurtable != null)
        {
            // Debug.Log($"按键 [{binding.triggerKey}]：对 {binding.target.name} 造成 {binding.damage} 点伤害！");
            hurtable.TakeDamage(binding.damage);
        }
        else
        {
            Debug.LogWarning($"❗目标 {binding.target.name} 未实现 IHurtable 接口！");
        }
    }



}
