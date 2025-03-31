using System.Collections.Generic;
using UnityEngine;
using System;

public class CatchDetectorTool : MonoBehaviour
{
    // 事件：每次更新时发布
    public event Action<List<GameObject>> OnDetectedObjectsUpdated;

    // 检测间隔（每5帧检测一次）
    private int _frameCounter = 0;

    // 触发器碰撞盒
    private Collider _triggerCollider;

    // 临时存储检测到的对象
    public List<GameObject> _detectedObjects = new();

    private void Start()
    {
        // 获取挂载的碰撞盒
        _triggerCollider = GetComponent<Collider>();

        // 确保碰撞盒是触发器
        if (_triggerCollider != null)
        {
            _triggerCollider.isTrigger = true;
        }
        else
        {
            Debug.LogError("No Collider found on this GameObject!");
        }
    }

    private void Update()
    {
        // 每5帧检测一次
        _frameCounter++;
        if (_frameCounter < 5) return;
        _frameCounter = 0;
        UpdateDetectedObjects();
    }

    // 更新检测到的对象列表
    private void UpdateDetectedObjects()
    {
        // 清空当前列表
        _detectedObjects.Clear();

        // 获取所有与触发器碰撞盒重叠的碰撞器
        var colliders = Physics.OverlapBox(_triggerCollider.bounds.center, _triggerCollider.bounds.extents, _triggerCollider.transform.rotation);

        foreach (var col in colliders)
        {
            // 排除带有指定标签的对象
            if (!col.CompareTag("Uncatchable") && col.gameObject.layer != LayerMask.NameToLayer("Floor"))
            {
                _detectedObjects.Add(col.gameObject);
            }
        }

        // 发布事件，通知列表已更新
        OnDetectedObjectsUpdated?.Invoke(_detectedObjects);
    }
}
