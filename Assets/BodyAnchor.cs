using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  定位用的锚点
/// 在空物体上添加该脚本，作为定位锚点，将物体设置为子物体
/// </summary>
public class BodyAnchor : MonoBehaviour
{
    [SerializeField]
    private string _name;
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }
}
