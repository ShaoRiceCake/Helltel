using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Serialization;

public class CatchDetectorTool : MonoBehaviour
{
    public event Action<List<GameObject>> OnDetectedObjectsUpdated;
    public PriceShowTool priceShowPrefab;  // 单一UI告示牌预制体（包含状态切换功能）
    public float signHeight = 1f;          // 提示牌固定高度

    private int _frameCounter;
    public Collider triggerCollider;
    public List<GameObject> detectedObjects = new();
    private readonly Dictionary<GameObject, PriceShowTool> _objectSignMap = new();

    private void Start()
    {
        if (!triggerCollider)
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
        var newState = canAfford ? PriceShowTool.PurchaseState.Purchasable : PriceShowTool.PurchaseState.NonPurchasable;

        if (_objectSignMap.TryGetValue(targetObject, out var existingSign))
        {
            existingSign.SetState(newState);
            existingSign.SetText(item.itemPrice.ToString());
            return;
        }

        CreateSignForObject(targetObject, item.itemPrice, newState);
    }

    private void CreateSignForObject(GameObject targetObject, int price, PriceShowTool.PurchaseState state)
    {
        if (!priceShowPrefab || price == 0) return;

        var signPosition = new Vector3(
            targetObject.transform.position.x,
            targetObject.transform.position.y + signHeight,
            targetObject.transform.position.z
        );
        
        var sign = Instantiate(priceShowPrefab, signPosition, Quaternion.identity);
        sign.SetText(price.ToString());
        sign.SetState(state);
        sign.SetTarget(targetObject.transform);
        _objectSignMap[targetObject] = sign;
    }

    private void RemoveSignForObject(GameObject targetObject)
    {
        if (!_objectSignMap.TryGetValue(targetObject, out var sign)) return;
        
        if (sign)
        {
            Destroy(sign.gameObject);
        }
        _objectSignMap.Remove(targetObject);
    }
}