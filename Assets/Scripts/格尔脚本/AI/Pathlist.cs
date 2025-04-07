using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
namespace Helltal.Gelercat
{
    [ExecuteAlways] // 编辑器模式也能执行
    public class Pathlist: MonoBehaviour
    {
        public List<WayPoint> waypoints = new List<WayPoint>();
        [SerializeField] private bool alwaysDrawPath = true;
        [SerializeField] private bool drawAsLoop = false;
        [SerializeField] private bool drawNumbers = true;
        [SerializeField] private bool AutoRegiest = true;
        public Color debugColour = Color.white;

        private void OnValidate()
        {
            if (AutoRegiest)
            {
                AutoRegisterWaypoints();
            }
        }

        private void Update()
        {

        }
        void OnTransformChildrenChanged()
        {
            if (AutoRegiest)
            {
                AutoRegisterWaypoints(); // 当子物体发生变化时，自动注册
            }
        }
        public void AutoRegisterWaypoints()
        {
            var children = new List<Transform>();
            foreach (Transform child in transform)
            {
                if (child != null)
                    children.Add(child);
            }

            // 只保留有效子物体，更新 WayPoints
            waypoints.Clear();
            foreach (var child in children)
            {
                waypoints.Add(new WayPoint { point = child, isCheck = false });
            }
        }

        public virtual void ResetAllCheck()
        {
            bool allcheck = false;
            foreach (var w in waypoints)
            {
                if (w != null && w.isCheck)
                    allcheck = true;
            }

            if (allcheck)
            {
                foreach (var w in waypoints)
                {
                    if (w != null) w.Check(false);
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (alwaysDrawPath)
            {
                DrawPath();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!alwaysDrawPath)
            {
                DrawPath();
            }
        }

        public void DrawPath()
        {
            if (waypoints == null || waypoints.Count == 0) return;

            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null || waypoints[i].point == null) continue;

                GUIStyle labelStyle = new GUIStyle
                {
                    fontSize = 30,
                    normal = { textColor = debugColour }
                };

                if (drawNumbers)
                    Handles.Label(waypoints[i].point.position, i.ToString(), labelStyle);

                if (i >= 1 && waypoints[i - 1] != null && waypoints[i - 1].point != null)
                {
                    Gizmos.color = debugColour;
                    Gizmos.DrawLine(waypoints[i - 1].point.position, waypoints[i].point.position);
                }
            }

            // Loop
            if (drawAsLoop && waypoints.Count > 1)
            {
                var first = waypoints[0];
                var last = waypoints[waypoints.Count - 1];
                if (first != null && last != null && first.point != null && last.point != null)
                {
                    Gizmos.color = debugColour;
                    Gizmos.DrawLine(last.point.position, first.point.position);
                }
            }
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
}