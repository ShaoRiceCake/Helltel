using UnityEngine;
using System.Collections.Generic;

public class ObiEmitterTester : MonoBehaviour
{
    [Header("发射位置")]
    public List<Transform> emissionPositions = new List<Transform>();

    [Header("发射参数随机区间")]
    public Vector2Int countRange = new Vector2Int(5, 10);
    public Vector2 speedRange = new Vector2(1f, 3f);
    public Vector2 randomnessRange = new Vector2(0.1f, 0.5f);

    [Header("发射设置")]
    public float emissionInterval = 0.5f;
    public bool loopEmission = false; // 是否循环发射
    public bool randomRotation = false; // 是否使用随机旋转

    [Header("引用")]
    public ObiEmitterManager emitterManager;

    private float _timer;
    private int _currentIndex;
    private bool _isCompleted = false;

    private void Start()
    {
        if (emitterManager == null)
        {
            Debug.LogError("ObiEmitterTester requires an ObiEmitterManager component reference");
            return;
        }

        if (emissionPositions.Count != 0) return;
        Debug.LogWarning("No emission positions assigned. Adding current transform as default.");
        emissionPositions.Add(transform);
    }

    private void Update()
    {
        if (!emitterManager || emissionPositions.Count == 0 || _isCompleted) return;

        _timer += Time.deltaTime;
        if (!(_timer >= emissionInterval)) return;
        _timer = 0f;
        EmitAtNextPosition();
    }

    private void EmitAtNextPosition()
    {
        if (_currentIndex >= emissionPositions.Count)
        {
            if (loopEmission)
            {
                _currentIndex = 0;
            }
            else
            {
                _isCompleted = true;
                Debug.Log("Emission sequence completed.");
                return;
            }
        }

        var position = emissionPositions[_currentIndex].position;
        var rotation = randomRotation ? Random.rotation : emissionPositions[_currentIndex].rotation;

        // 从区间中随机取值
        var count = Random.Range(countRange.x, countRange.y + 1);
        var speed = Random.Range(speedRange.x, speedRange.y);
        var randomness = Random.Range(randomnessRange.x, randomnessRange.y);

        // 使用API方法发射
        emitterManager.EmitAtPosition(position, rotation, count, speed, randomness);

        _currentIndex++;
    }

    // 重置发射器状态
    public void ResetEmitter()
    {
        _currentIndex = 0;
        _timer = 0f;
        _isCompleted = false;
    }
    
}