using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  黑雾控制器
/// </summary>

public class BlackForgController : MonoBehaviour
{

    // 
    public ParticleSystem fogEffect; // 黑雾粒子系统
    private List<Transform> players = new List<Transform>();

    [Header("第一阶段黑雾伤害")] public int Stage1FogDamage = 1; // 第一阶段黑雾伤害
    [Header("第二阶段黑雾伤害")] public int Stage2FogDamage = 2; // 第二阶段黑雾伤害


    [Header("第一阶段黑雾扩散速度")] public float Stage1FogSpeed = 1.5f; // 第1阶段黑雾速度
    [Header("第二阶段黑雾扩散速度")] public float Stage2FogSpeed = 2.0f; // 第2阶段黑雾速度
    [Header("黑雾最大扩散范围")] public float fogExpandRange = 5f; // 黑雾最大扩散范围


    [Header("第一阶段黑雾伤害间隔")] public float Stage1FogDamageInterval = 0.5f; // 第一阶段黑雾伤害间隔
    [Header("第二阶段黑雾伤害间隔")] public float Stage2FogDamageInterval = 0.5f; // 第二阶段黑雾伤害间隔




    private float currentRadius = 0f;
    private float fogTimer = 0f;
    private float damageTimer = 0f;
    private int currentDamage = 0;
    private float currentSpeed = 0f;
    private float currentDamageInterval = 0.5f;
    private bool isActive = false;
    private bool isStage2 = false;
    public void StartFogStage1()
    {
        Debug.Log("StartFogStage1");
        isActive = true;
        isStage2 = false;
        currentRadius = 0f;
        fogTimer = 0f;
        currentSpeed = Stage1FogSpeed;
        currentDamage = Stage1FogDamage;
        currentDamageInterval = Stage1FogDamageInterval;
        fogEffect.Play();
    }

    public void StartFogStage2()
    {
        Debug.Log("StartFogStage2");
        isActive = true;
        isStage2 = true;
        currentSpeed = Stage2FogSpeed * 3f;
        currentDamage = Stage2FogDamage * 2;
        currentDamageInterval = Stage2FogDamageInterval;
        // 不再重置 radius，继续在已有基础上扩张
    }

    public void StopFog()
    {
        Debug.Log("StopFog");
        isActive = false;
        fogEffect.Stop();
    }

    void Awake()
    {
        fogEffect.Stop(); // 确保粒子系统在开始时停止
        var shape = fogEffect.shape;
        shape.radius = 0f; // 设置初始半径为0
    }
    void Update()
    {
        if (!isActive) return;

        fogTimer += Time.deltaTime;
        currentRadius += currentSpeed * Time.deltaTime;
        currentRadius = Mathf.Min(currentRadius, fogExpandRange);

        var shape = fogEffect.shape;
        shape.radius = currentRadius;

        damageTimer += Time.deltaTime;
        if (damageTimer >= currentDamageInterval)
        {
            ApplyFogDamageToPlayers();
            damageTimer = 0f;
        }
    }

    private void ApplyFogDamageToPlayers()
    {
        foreach (var player in players)
        {
            float dist = Vector3.Distance(player.position, transform.position);
            if (dist <= currentRadius)
            {
                // TODO：调用玩家受伤方法
                //这里用的是单机的玩家扣血，且只是扣血，没有相应表现
                GameController.Instance.DeductHealth(currentDamage);

            }
        }
    }

    void OnDrawGizmos()
    {
        // 绘制黑雾范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, currentRadius);
    }
}
