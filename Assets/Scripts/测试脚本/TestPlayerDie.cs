using UnityEngine;

public class TestHealthDeduction : MonoBehaviour
{
    [Header("伤害设置")]
    [Tooltip("每次按键扣除的生命值")]
    public int damageAmount = 10;
    
    [Tooltip("按键输入检测")]
    public KeyCode damageKey = KeyCode.C;
    
    [Tooltip("治疗按键")]
    public KeyCode healKey = KeyCode.V;
    
    [Tooltip("每次按键恢复的生命值")]
    public int healAmount = 10;


    void Update()
    {
        // 检测C键按下 - 扣血
        if (Input.GetKeyDown(damageKey))
        {
            ApplyDamage(damageAmount);
        }
        
        // 检测V键按下 - 治疗
        if (Input.GetKeyDown(healKey))
        {
            Heal(healAmount);
        }
    }

    void ApplyDamage(int amount)
    {

        GameController.Instance.DeductHealth(amount);
    }

    void Heal(int amount)
    {
        GameController.Instance.AddHealth(amount);
    }
}