using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using TMPro;

public class CatchDetectorTool : MonoBehaviour
{
    public event Action<List<GameObject>> OnDetectedObjectsUpdated;
    public GameObject warningSignPrefab;    // 警告提示牌（金额不足）
    public GameObject approvedSignPrefab;  // 许可提示牌（金额足够）
    public float signHeight = 1f;          // 提示牌固定高度

    private int _frameCounter;
    public Collider triggerCollider;
    public List<GameObject> detectedObjects = new();
    private readonly Dictionary<GameObject, GameObject> _objectSignMap = new();

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
        UpdateSignsPositionAndRotation();
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
            
            var item = col.GetComponent<ItemBase>();
            detectedObjects.Add(col.gameObject);
            currentDetections.Add(col.gameObject);
            
            if (_objectSignMap.ContainsKey(col.gameObject) && (item == null || item.itemPrice == 0 || item.IsPurchase))
            {
                RemoveSignForObject(col.gameObject);
                continue;
            }
            
            if (item && item.itemPrice > 0 && !item.IsPurchase)
            {
                UpdateOrCreateSign(col.gameObject, item);
            }
        }

        foreach (var oldObj in previousDetections.Where(oldObj => !currentDetections.Contains(oldObj)))
        {
            RemoveSignForObject(oldObj);
        }

        OnDetectedObjectsUpdated?.Invoke(detectedObjects);
    }

    private void UpdateOrCreateSign(GameObject targetObject, ItemBase item)
    {
        var canAfford = GameController.Instance.GetMoney() >= item.itemPrice;
        var correctPrefab = canAfford ? approvedSignPrefab : warningSignPrefab;
        var correctTag = canAfford ? "ApprovedSign" : "WarningSign";

        if (_objectSignMap.TryGetValue(targetObject, out var existingSign))
        {
            if (existingSign.CompareTag(correctTag)) 
            {
                return; 
            }
            RemoveSignForObject(targetObject);
        }

        CreateSignForObject(targetObject, item.itemPrice, correctPrefab, correctTag);
    }

    private void CreateSignForObject(GameObject targetObject, int price, GameObject prefab, string tag)
    {
        if (!prefab || price == 0) return;

        var signPosition = new Vector3(
            targetObject.transform.position.x,
            targetObject.transform.position.y + signHeight,
            targetObject.transform.position.z
        );
        
        var sign = Instantiate(prefab, signPosition, Quaternion.identity);
        sign.tag = tag;
        _objectSignMap[targetObject] = sign;

    }

    private void RemoveSignForObject(GameObject targetObject)
    {
        if (!_objectSignMap.TryGetValue(targetObject, out var sign)) return;
        
        if (sign)
        {
            Destroy(sign);
        }
        _objectSignMap.Remove(targetObject);
    }
    
    private void UpdateSignsPositionAndRotation()
    {
        if (_objectSignMap.Count == 0) return;
        
        foreach (var kvp in _objectSignMap.ToList())
        {
            if (!kvp.Value || !kvp.Key)
            {
                _objectSignMap.Remove(kvp.Key);
                continue;
            }
            
            var targetPos = kvp.Key.transform.position;
            kvp.Value.transform.position = Vector3.Lerp(
                kvp.Value.transform.position,
                new Vector3(targetPos.x, targetPos.y + signHeight, targetPos.z),
                10f * Time.deltaTime
            );
            
            kvp.Value.transform.Rotate(100 * Time.deltaTime, 100 * Time.deltaTime, 100 * Time.deltaTime, Space.World);
        }
    }
}