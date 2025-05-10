using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CatchDetectorTool : MonoBehaviour
{
    public event Action<List<GameObject>> OnDetectedObjectsUpdated;
    public GameObject signboardPrefab;

    private int _frameCounter;
    public Collider triggerCollider;
    public List<GameObject> detectedObjects = new();
    private Dictionary<GameObject, GameObject> _objectSignMap = new(); 

    private void Start()
    {
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
        else
        {
            Debug.LogError("No Collider found on this GameObject!");
        }
    }

    private void FixedUpdate()
    {
        _frameCounter++;
        if (_frameCounter < 5) return;
        _frameCounter = 0;
        UpdateDetectedObjects();
        UpdateSignsRotation(); // 新增：更新所有告示牌的旋转
    }

    private void UpdateDetectedObjects()
    {
        var currentDetections = new List<GameObject>();

        var previousDetections = new List<GameObject>(detectedObjects);
        detectedObjects.Clear();

        var colliders = Physics.OverlapBox(triggerCollider.bounds.center, triggerCollider.bounds.extents, triggerCollider.transform.rotation);

        foreach (var col in colliders)
        {
            if (!col.TryGetComponent(out IGrabbable _)) continue;
            detectedObjects.Add(col.gameObject);
            currentDetections.Add(col.gameObject);
                
            var item = col.GetComponent<ItemBase>(); 
            if (!item || item.itemPrice == 0) continue;
            if (!_objectSignMap.ContainsKey(col.gameObject))
            {
                CreateSignForObject(col.gameObject, item.itemPrice);
            }
        }

        foreach (var oldObj in previousDetections.Where(oldObj => !currentDetections.Contains(oldObj)))
        {
            if (!_objectSignMap.TryGetValue(oldObj, out var sign)) continue;
            Destroy(sign);
            _objectSignMap.Remove(oldObj);
        }

        OnDetectedObjectsUpdated?.Invoke(detectedObjects);
    }

    private void CreateSignForObject(GameObject targetObject, int price)
    {
        if (!signboardPrefab)
        {
            Debug.LogWarning("Signboard prefab is not assigned!");
            return;
        }

        var signPosition = targetObject.transform.position + Vector3.up * 1f;
        
        // 计算旋转使告示牌的 -Y 轴指向当前对象
        var directionToDetector = transform.position - signPosition;
        var targetRotation = Quaternion.LookRotation(Vector3.forward, -directionToDetector);
        
        var sign = Instantiate(signboardPrefab, signPosition, targetRotation);
        
        _objectSignMap[targetObject] = sign;
    }

    // 新增方法：更新所有告示牌的旋转
    private void UpdateSignsRotation()
    {
        foreach (var kvp in _objectSignMap.Where(kvp => kvp.Value))
        {
            kvp.Value.transform.Rotate(Vector3.left, 300 * Time.deltaTime);
        }
    }
}