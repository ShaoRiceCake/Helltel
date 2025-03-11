using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connector : MonoBehaviour
{
    public Vector2 _size = Vector2.one * 4f;
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector2 _halfSize = _size * 0.5f;
        Vector3 _offset = transform.position + transform.up * _halfSize.y;
        Gizmos.DrawLine(_offset,_offset +transform.forward); 

        //定义向上的向量和向右的向量
        Vector3 _top = transform.up*_size.y;
        Vector3 _side = transform.right *_halfSize.x;
        //定义四个角的向量
        Vector3 _topRight = transform.position + _top + _side;
        Vector3 _topLeft = transform.position + _top - _side;
        Vector3 _bottomRight = transform.position + _side;
        Vector3 _bottomLeft = transform.position - _side;
        //
        Gizmos.DrawLine(_topRight,_topLeft);
        Gizmos.DrawLine(_topRight,_bottomRight);
        Gizmos.DrawLine(_topLeft,_bottomLeft);
        Gizmos.DrawLine(_bottomRight,_bottomLeft);
        //
        Gizmos.color *= 0.6f;
        Gizmos.DrawLine(_topRight,_offset);
        Gizmos.DrawLine(_topLeft,_offset);
        Gizmos.DrawLine(_bottomRight,_offset);
        Gizmos.DrawLine(_bottomLeft,_offset);
        
    }
}
