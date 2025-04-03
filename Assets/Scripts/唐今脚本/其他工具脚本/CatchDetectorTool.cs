using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;

public class CatchDetectorTool : MonoBehaviour
{
    public event Action<List<GameObject>> OnDetectedObjectsUpdated;

    private int _frameCounter;

    public Collider triggerCollider;

    public List<GameObject> detectedObjects = new();

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

    private void Update()
    {
        _frameCounter++;
        if (_frameCounter < 5) return;
        _frameCounter = 0;
        UpdateDetectedObjects();
    }

    private void UpdateDetectedObjects()
    {
        detectedObjects.Clear();

        var colliders = Physics.OverlapBox(triggerCollider.bounds.center, triggerCollider.bounds.extents, triggerCollider.transform.rotation);

        foreach (var col in colliders)
        {
            if (!col.CompareTag("Uncatchable") && col.gameObject.layer != LayerMask.NameToLayer("Floor"))
            {
                detectedObjects.Add(col.gameObject);
            }
        }

        OnDetectedObjectsUpdated?.Invoke(detectedObjects);
    }
}
