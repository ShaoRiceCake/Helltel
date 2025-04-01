
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class Pathlist: MonoBehaviour
{

    public List<WayPoint> waypoints;//寻路路径点
    [SerializeField]
    private bool alwaysDrawPath;
    [SerializeField]
    private bool drawAsLoop;
    [SerializeField]
    private bool drawNumbers;
    public Color debugColour = Color.white;


    public virtual void ResetAllCheck()
    {
        bool allcheck = false;

        foreach (var w in waypoints)
        {
            if (w.isCheck)
            {
                allcheck = true;
            }
        }
        if (allcheck)
        {
            foreach (var w in waypoints)
            {
                w.Check(false);
            }
        }

    }


#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (alwaysDrawPath)
        {
            DrawPath();//绘制路径
        }
    }

    public void DrawPath()
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.fontSize = 30;
            labelStyle.normal.textColor = debugColour;
            if (drawNumbers)
                Handles.Label(waypoints[i].point.position, i.ToString(), labelStyle);

            if (i >= 1)
            {
                Gizmos.color = debugColour;
                Gizmos.DrawLine(waypoints[i - 1].point.position, waypoints[i].point.position);

                if (drawAsLoop)
                    Gizmos.DrawLine(waypoints[waypoints.Count - 1].point.position, waypoints[0].point.position);

            }
        }
    }
    public void OnDrawGizmosSelected()
    {
        if (alwaysDrawPath)
            return;
        else
            DrawPath();
    }
#endif
}

[Serializable]
public class WayPoint
{
    public Transform point;
    public bool isCheck;

    public void Check(bool check)
    {
        isCheck = check;
    }
}
