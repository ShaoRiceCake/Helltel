// using System.Collections.Generic;
// using UnityEngine;

// /// <summary>
// ///  黑雾控制器
// /// </summary>

// public class BlackForgController : MonoBehaviour
// {

//     // 
//     public ParticleSystem fogEffect; // 黑雾粒子系统
//     private List<Transform> players = new List<Transform>();

//     [Header("第一阶段黑雾伤害")] public int Stage1FogDamage = 1; // 第一阶段黑雾伤害
//     [Header("第二阶段黑雾伤害")] public int Stage2FogDamage = 2; // 第二阶段黑雾伤害


//     [Header("第一阶段黑雾扩散速度")] public float Stage1FogSpeed = 1.5f; // 第1阶段黑雾速度
//     [Header("第二阶段黑雾扩散速度")] public float Stage2FogSpeed = 2.0f; // 第2阶段黑雾速度
//     [Header("黑雾最大扩散范围")] public float fogExpandRange = 5f; // 黑雾最大扩散范围


//     [Header("第一阶段黑雾伤害间隔")] public float Stage1FogDamageInterval = 0.5f; // 第一阶段黑雾伤害间隔
//     [Header("第二阶段黑雾伤害间隔")] public float Stage2FogDamageInterval = 0.5f; // 第二阶段黑雾伤害间隔




//     private float currentRadius = 0f;
//     private float fogTimer = 0f;
//     private float damageTimer = 0f;
//     private int currentDamage = 0;
//     private float currentSpeed = 0f;
//     private float currentDamageInterval = 0.5f;
//     private bool isActive = false;
//     private bool isStage2 = false;
//     public void StartFogStage1()
//     {
//         Debug.Log("StartFogStage1");
//         isActive = true;
//         isStage2 = false;
//         currentRadius = 0f;
//         fogTimer = 0f;
//         currentSpeed = Stage1FogSpeed;
//         currentDamage = Stage1FogDamage;
//         currentDamageInterval = Stage1FogDamageInterval;
//         fogEffect.Play();
//     }

//     public void StartFogStage2()
//     {
//         Debug.Log("StartFogStage2");
//         isActive = true;
//         isStage2 = true;
//         currentSpeed = Stage2FogSpeed * 3f;
//         currentDamage = Stage2FogDamage * 2;
//         currentDamageInterval = Stage2FogDamageInterval;
//         // 不再重置 radius，继续在已有基础上扩张
//     }

//     public void StopFog()
//     {
//         Debug.Log("StopFog");
//         isActive = false;
//         fogEffect.Stop();
//     }

//     void Awake()
//     {
//         fogEffect.Stop(); // 确保粒子系统在开始时停止
//         var shape = fogEffect.shape;
//         shape.radius = 0f; // 设置初始半径为0
//     }
//     void Update()
//     {
//         if (!isActive) return;

//         fogTimer += Time.deltaTime;
//         currentRadius += currentSpeed * Time.deltaTime;
//         currentRadius = Mathf.Min(currentRadius, fogExpandRange);

//         var shape = fogEffect.shape;
//         shape.radius = currentRadius;

//         damageTimer += Time.deltaTime;
//         if (damageTimer >= currentDamageInterval)
//         {
//             ApplyFogDamageToPlayers();
//             damageTimer = 0f;
//         }
//     }

//     private void ApplyFogDamageToPlayers()
//     {
//         foreach (var player in players)
//         {
//             float dist = Vector3.Distance(player.position, transform.position);
//             if (dist <= currentRadius)
//             {
//                 // TODO：调用玩家受伤方法
//                 //这里用的是单机的玩家扣血，且只是扣血，没有相应表现
//                 GameController.Instance.DeductHealth(currentDamage);

//             }
//         }
//     }

//     void OnDrawGizmos()
//     {
//         // 绘制黑雾范围
//         Gizmos.color = Color.red;
//         Gizmos.DrawWireSphere(transform.position, currentRadius);
//     }
// }



using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 黑雾控制器（重构版）
/// </summary>
public class BlackForgController : MonoBehaviour
{
    [Header("黑雾粒子系统")]
    public ParticleSystem fogEffect;

    [Header("黑雾最大半径")]
    public float fogExpandRange = 5f;

    [Header("阶段1参数（孤独）")]
    public float Stage1FogSpeed = 1.5f;
    public int Stage1FogDamage = 1;
    public float Stage1FogDamageInterval = 0.5f;

    [Header("阶段2参数（死亡）")]
    public float Stage2FogSpeed = 4.5f; // 通常是阶段1的 ×3
    public int Stage2FogDamage = 4;     // 通常是阶段1的 ×2
    public float Stage2FogDamageInterval = 0.5f;

    [Header("高兴状态消散参数")]
    public float fadeOutSpeed = 2f; // 每秒收缩米数

    // 状态控制
    private float currentRadius = 0f;
    private float currentSpeed = 0f;
    private int currentDamage = 0;
    private float currentDamageInterval = 0.5f;
    private float damageTimer = 0f;

    private bool isActive = false;
    private bool isStage2 = false;
    private bool isFadingOut = false;

    private List<Transform> players = new List<Transform>();

    public float CurrentRadius => currentRadius; // 提供外部访问

    void Awake()
    {
        fogEffect.Stop();
        var shape = fogEffect.shape;
        shape.radius = 0f;
    }

    void Update()
    {
        if (isActive)
        {
            // 黑雾扩散
            currentRadius += currentSpeed * Time.deltaTime;
            currentRadius = Mathf.Min(currentRadius, fogExpandRange);

            // 黑雾伤害
            damageTimer += Time.deltaTime;
            if (damageTimer >= currentDamageInterval)
            {
                ApplyFogDamageToPlayers();
                damageTimer = 0f;
            }
        }
        else if (isFadingOut)
        {
            // 黑雾收缩（淡出）
            currentRadius -= fadeOutSpeed * Time.deltaTime;
            if (currentRadius <= 0f)
            {
                currentRadius = 0f;
                fogEffect.Stop();
                isFadingOut = false;
            }
        }

        // 同步粒子系统半径
        var shape = fogEffect.shape;
        shape.radius = currentRadius;
    }

    public void StartFogStage1()
    {
        Debug.Log("黑雾阶段1启动");
        isActive = true;
        isStage2 = false;
        isFadingOut = false;

        currentRadius = 0f;
        currentSpeed = Stage1FogSpeed;
        currentDamage = Stage1FogDamage;
        currentDamageInterval = Stage1FogDamageInterval;

        fogEffect.Play();
    }

    public void StartFogStage2()
    {
        Debug.Log("黑雾阶段2启动：加速扩散与伤害翻倍");
        isActive = true;
        isStage2 = true;
        isFadingOut = false;

        currentSpeed = Stage2FogSpeed;
        currentDamage = Stage2FogDamage;
        currentDamageInterval = Stage2FogDamageInterval;
        // 注意：不重置 currentRadius，继续在已有基础上扩展
    }

    public void FadeOutFog()
    {
        Debug.Log("黑雾开始淡出");
        isActive = false;
        isFadingOut = true;
        // fogEffect 不停，由 radius 驱动衰减后自动关闭
    }

    public void StopFogImmediate()
    {
        Debug.Log("黑雾立即停止");
        isActive = false;
        isFadingOut = false;
        fogEffect.Stop();
        currentRadius = 0f;
    }

    private void ApplyFogDamageToPlayers()
    {
        foreach (var player in players)
        {
            if (player == null) continue;

            float dist = Vector3.Distance(player.position, transform.position);
            if (dist <= currentRadius)
            {
                GameController.Instance.DeductHealth(currentDamage);
                // 可拓展玩家具体受伤表现
            }
        }
    }

    public void RegisterPlayer(Transform player)
    {
        if (!players.Contains(player)) players.Add(player);
    }

    public void UnregisterPlayer(Transform player)
    {
        if (players.Contains(player)) players.Remove(player);
    }

    void OnDrawGizmos()
    {
        if (isActive || isFadingOut)
        {
            Gizmos.color = isStage2 ? Color.magenta : Color.red;
            Gizmos.DrawWireSphere(transform.position, currentRadius);
        }
    }
}
