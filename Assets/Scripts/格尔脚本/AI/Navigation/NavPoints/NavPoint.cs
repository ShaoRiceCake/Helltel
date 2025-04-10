using System;
using UnityEngine;

public class NavPoint : MonoBehaviour
{
    public static event Action<NavPoint> OnNavPointCreated;
    public static event Action<NavPoint> OnNavPointDestroyed;
    public static event Action<NavPoint> OnNavPointMoved;

    public string navPointID = Guid.NewGuid().ToString(); // Î¨Ò»±êÊ¶·û

    private Vector3 lastPosition;

    private void OnEnable()
    {
        lastPosition = transform.position;
        OnNavPointCreated?.Invoke(this);
    }

    private void OnDisable()
    {
        OnNavPointDestroyed?.Invoke(this);
    }

    private void LateUpdate()
    {
        if(transform.position != lastPosition)
        {
            lastPosition = transform.position;
            OnNavPointMoved?.Invoke(this);
        }
    }

    void OnDrawGizmos()
    {

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }


    public Transform GetTransform()
    {
        return transform;
    }
    public string GetID()
    {
        return navPointID;
    }

}

