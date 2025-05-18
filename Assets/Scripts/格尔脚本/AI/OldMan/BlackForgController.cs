using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 黑雾控制器（重构版）
/// </summary>
public class BlackForgController : MonoBehaviour
{

    private ParticleSystem _fogParticleSystem;
    private SphereCollider _fogVolumeCollider;


    [Header("黑雾最大半径")]
    public float fogExpandRange = 5f;

    [Header("阶段1参数（孤独）")]
    public float Stage1FogSpeed = 1.5f;
    public int Stage1FogDamage = 1;
    public float Stage1FogDamageInterval = 0.5f;

    [Header("阶段2参数（死亡）")]
    public float Stage2FogSpeed = 4.5f;
    public int Stage2FogDamage = 4;
    public float Stage2FogDamageInterval = 0.5f;

    [Header("高兴状态消散参数")]
    public float fadeOutSpeed = 2f;

    private float currentRadius = 0f;
    private float currentSpeed = 0f;
    private int currentDamage = 0;
    private float currentDamageInterval = 0.5f;
    private float damageTimer = 0f;

    private bool isActive = false;
    private bool isStage2 = false;
    private bool isFadingOut = false;

    private List<Transform> players = new List<Transform>();
    public float CurrentRadius => currentRadius;
    void Awake()
    {
        _fogParticleSystem = GetComponent<ParticleSystem>();
        _fogVolumeCollider = GetComponentInChildren<SphereCollider>();
        if (_fogVolumeCollider != null)
        {
            _fogVolumeCollider.isTrigger = true;
            _fogVolumeCollider.radius = 0f;
        }
        else
        {
            Debug.LogError("没有找到 SphereCollider 组件");
        }
        var main = _fogParticleSystem.main;

        main.startSpeed = new ParticleSystem.MinMaxCurve(20f);

    }

    void Update()
    {
        if (isActive)
        {
            currentRadius += currentSpeed * Time.deltaTime;
            currentRadius = Mathf.Min(currentRadius, fogExpandRange);

            damageTimer += Time.deltaTime;
            if (damageTimer >= currentDamageInterval)
            {
                ApplyFogDamageToPlayers();
                damageTimer = 0f;
            }
        }
        else if (isFadingOut)
        {
            currentRadius -= fadeOutSpeed * Time.deltaTime;
            if (currentRadius <= 0f)
            {
                currentRadius = 0f;
                isFadingOut = false;
                _fogParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        // 同步黑雾大小
        var main = _fogParticleSystem.main;
        main.startSize = new ParticleSystem.MinMaxCurve(currentRadius);
        // main.startSpeed = new ParticleSystem.MinMaxCurve(currentRadius);

        if (_fogVolumeCollider != null)
        {
            _fogVolumeCollider.radius = currentRadius;
        }
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
        _fogParticleSystem.Play();
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
        // 不重置半径，继续扩散
    }

    public void FadeOutFog()
    {
        Debug.Log("黑雾开始淡出");
        isActive = false;
        isFadingOut = true;
        // 粒子系统继续播放，直到 currentRadius 收缩为 0
    }

    public void StopFogImmediate()
    {
        Debug.Log("黑雾立即停止");
        isActive = false;
        isFadingOut = false;
        currentRadius = 0f;
        _fogParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (_fogVolumeCollider != null)
            _fogVolumeCollider.radius = 0f;
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
                // 可扩展具体玩家行为
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
