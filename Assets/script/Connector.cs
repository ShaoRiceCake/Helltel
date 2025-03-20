using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connector : MonoBehaviour
{
    [SerializeField]
    private Vector2 _size = Vector2.one * 4f;
    [SerializeField]
    public bool isConnected = false;
    private bool isPlaying;
    void Start()
    {
        isPlaying = true;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = isConnected? Color.green : Color.red;
        if(isPlaying == false){Gizmos.color = Color.cyan;}
        
        Vector2 halfSize = _size * 0.5f;
        Vector3 offset = transform.position + transform.up * halfSize.y;
        Gizmos.DrawLine(offset,offset +transform.forward); 

        //定义向上的向量和向右的向量
        Vector3 top = transform.up*_size.y;
        Vector3 side = transform.right *halfSize.x;
        //定义四个角的向量
        Vector3 topRight = transform.position + top + side;
        Vector3 topLeft = transform.position + top - side;
        Vector3 bottomRight = transform.position + side;
        Vector3 bottomLeft = transform.position - side;
        
        Gizmos.DrawLine(topRight,topLeft);
        Gizmos.DrawLine(topRight,bottomRight);
        Gizmos.DrawLine(topLeft,bottomLeft);
        Gizmos.DrawLine(bottomRight,bottomLeft);
        
        Gizmos.color *= 0.6f;
        Gizmos.DrawLine(topRight,offset);
        Gizmos.DrawLine(topLeft,offset);
        Gizmos.DrawLine(bottomRight,offset);
        Gizmos.DrawLine(bottomLeft,offset);
        
    }
}
