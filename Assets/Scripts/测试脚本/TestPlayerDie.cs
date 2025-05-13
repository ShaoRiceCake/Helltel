using UnityEngine;
using UnityEngine.UI;

public class TestHealthDeduction : MonoBehaviour
{
    [Header("调试面板设置")]
    public bool showDebugPanel = true;
    public KeyCode toggleKey = KeyCode.F12;
    
    [Header("血量设置")]
    public Slider healthSlider;
    public int maxHealth = 100;
    public int healthChangeStep = 10;
    
    [Header("绩效设置")]
    public Slider performanceSlider;
    public int maxPerformance = 100;
    public int performanceChangeStep = 10;
    
    [Header("金币设置")]
    public Slider moneySlider;
    public int maxMoney = 1000;
    public int moneyChangeStep = 100;
    
    private Rect debugWindowRect = new Rect(20, 20, 300, 400);
    private GameDataModel gameData;

    void Start()
    {
        gameData = GameController.Instance._gameData;
        
        // 初始化滑块最大值
        if (healthSlider) healthSlider.maxValue = maxHealth;
        if (performanceSlider) performanceSlider.maxValue = maxPerformance;
        if (moneySlider) moneySlider.maxValue = maxMoney;
    }

    void Update()
    {
        // 切换调试面板显示
        if (Input.GetKeyDown(KeyCode.X))
        {
            showDebugPanel = !showDebugPanel;
        }

        // 更新滑块值
        if (healthSlider) healthSlider.value = gameData.Health;
        if (performanceSlider) performanceSlider.value = gameData.Performance;
        if (moneySlider) moneySlider.value = gameData.Money;
    }

    void OnGUI()
    {
        if (!showDebugPanel) return;

        debugWindowRect = GUI.Window(0, debugWindowRect, DebugWindow, "游戏调试面板");
    }

    void DebugWindow(int windowID)
    {
        GUILayout.BeginVertical();
        
        // 血量控制
        GUILayout.Label($"血量: {gameData.Health}/{maxHealth}");
        gameData.Health = (int)GUILayout.HorizontalSlider(gameData.Health, 0, maxHealth);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button($"-{healthChangeStep}")) GameController.Instance.DeductHealth(healthChangeStep);
        if (GUILayout.Button($"+{healthChangeStep}")) GameController.Instance.AddHealth(healthChangeStep);
        GUILayout.EndHorizontal();
        
        // 绩效控制
        GUILayout.Label($"绩效: {gameData.Performance}/{maxPerformance}");
        gameData.Performance = (int)GUILayout.HorizontalSlider(gameData.Performance, 0, maxPerformance);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button($"-{performanceChangeStep}")) GameController.Instance.DeductPerformance(performanceChangeStep);
        if (GUILayout.Button($"+{performanceChangeStep}")) GameController.Instance.AddPerformance(performanceChangeStep);
        GUILayout.EndHorizontal();
        
        // 金币控制
        GUILayout.Label($"金币: {gameData.Money}/{maxMoney}");
        gameData.Money = (int)GUILayout.HorizontalSlider(gameData.Money, 0, maxMoney);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button($"-{moneyChangeStep}")) GameController.Instance.DeductMoney(moneyChangeStep);
        if (GUILayout.Button($"+{moneyChangeStep}")) GameController.Instance.AddMoney(moneyChangeStep);
        GUILayout.EndHorizontal();
        
        // 重置按钮
        if (GUILayout.Button("重置所有数据"))
        {
            GameController.Instance._gameData.ResetData();
        }
        
        GUILayout.EndVertical();
        
        // 允许拖动窗口
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    // 供UI滑块调用的方法
    public void OnHealthSliderChanged(float value)
    {
        gameData.Health = (int)value;
    }
    
    public void OnPerformanceSliderChanged(float value)
    {
        gameData.Performance = (int)value;
    }
    
    public void OnMoneySliderChanged(float value)
    {
        gameData.Money = (int)value;
    }
}