using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class DontDestroyTriggerZone : MonoBehaviour
{
    public Collider triggerCollider;
    private readonly HashSet<GameObject> _objectsInTrigger = new();

    private void Awake()
    {
        if (triggerCollider == null) triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    private void Start()
    {
        StartCoroutine(DelayedInitialCheck());
    }

    private IEnumerator DelayedInitialCheck()
    {
        yield return null; 
        DetectInitialObjects();
    }

    private void DetectInitialObjects()
    {
        if (!triggerCollider) return;

        var colliders = Physics.OverlapBox(
            triggerCollider.bounds.center,
            triggerCollider.bounds.extents,
            triggerCollider.transform.rotation);

        foreach (Collider col in colliders)
        {
            if (col.gameObject == this.gameObject || !_objectsInTrigger.Add(col.gameObject)) continue;
            DontDestroyManager.Instance.AddObject(col.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == this.gameObject || !_objectsInTrigger.Add(other.gameObject)) return;
        DontDestroyManager.Instance.AddObject(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == this.gameObject || !_objectsInTrigger.Contains(other.gameObject)) return;
        _objectsInTrigger.Remove(other.gameObject);
        DontDestroyManager.Instance.RemoveObject(other.gameObject);
    }
}

