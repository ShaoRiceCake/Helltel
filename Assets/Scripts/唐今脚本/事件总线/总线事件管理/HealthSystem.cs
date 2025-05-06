// 生命值变动事件类

using UnityEngine;

public class HealthChangedEvent
{
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }
    public int ChangeAmount { get; private set; }

    public HealthChangedEvent(int current, int max, int change)
    {
        CurrentHealth = current;
        MaxHealth = max;
        ChangeAmount = change;
    }
}

// 发布者类框架
public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int _currentHealth;
    public int CurrentHealth => _currentHealth;

    private void Start()
    {
        _currentHealth = 50;
    }
    
    public void ChangeHealth(int amount)
    {
        var previousHealth = _currentHealth;
        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, maxHealth);
        
        // 发布生命值变动事件
        EventBus<HealthChangedEvent>.Publish(
            new HealthChangedEvent(
                _currentHealth, 
                maxHealth, 
                _currentHealth - previousHealth
            )
        );
        
        // Debug.Log($"生命值变动: {previousHealth} -> {_currentHealth}");
    }
    
    // // 测试用方法
    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.UpArrow))
    //     {
    //         ChangeHealth(10); // 增加生命值
    //     }
    //     else if (Input.GetKeyDown(KeyCode.DownArrow))
    //     {
    //         ChangeHealth(-10); // 减少生命值
    //     }
    // }
    
}