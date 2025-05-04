using System;
using UnityEngine;
using Obi;

[RequireComponent(typeof(ObiEmitter))]
public class CustomEmitterController : MonoBehaviour
{
    private ObiEmitter _emitter;
    private bool _hasEmitted;
    private bool _isFinished = false;

    private void Awake()
    {
        _emitter = GetComponent<ObiEmitter>();

        if (!_emitter)
        {
            Debug.LogError("未找到ObiEmitter组件");
        }
        
        _emitter.enabled = false;
    }

    private void Update()
    {
        // 测试
        if (Input.GetKeyDown(KeyCode.P))
        {
            Emit();
        }
        
        
        if (_isFinished) return;
        
        var allEmitted = _emitter.activeParticleCount == _emitter.particleCount;

        if (!allEmitted) return;
        _emitter.emissionMethod = ObiEmitter.EmissionMethod.MANUAL;
        _isFinished = true;
    }

    public void Emit()
    {
        if (_hasEmitted) return;
        
        EmitAllParticles();
        _hasEmitted = true;
    }

    /// <summary>
    /// 设置喷射速度（仅速度值，不改变方向）
    /// </summary>
    /// <param name="speedValue">速度值（标量）</param>
    public void SetEmissionSpeed(float speedValue)
    {
        if (_emitter == null) return;
    
        // 直接设置速度值
        _emitter.speed = Mathf.Max(1, speedValue); // 确保不小于0
    
        _emitter.UpdateEmitter();
    }

    /// <summary>
    /// 设置喷射随机性
    /// </summary>
    /// <param name="randomness">0-1之间的随机值</param>
    public void SetEmissionRandomness(float randomness)
    {
        if (_emitter == null) return;
        _emitter.randomDirection = Mathf.Clamp01(randomness);
        _emitter.UpdateEmitter();
    }

    private void EmitAllParticles()
    {
        _emitter.enabled = true;
        Debug.Log("发射血液");
    }
}