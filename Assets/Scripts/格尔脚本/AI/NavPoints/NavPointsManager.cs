using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Helltal.Gelercat
{
    [ExecuteAlways]
    public class NavPointsManager : MonoBehaviour
    {
        [SerializeField]
        private bool alwaysDrawPath = true;

        [SerializeField]
        private bool drawAsLoop = false;

        [SerializeField]
        private bool drawNumbers = true;

        public Color debugColour = Color.white;

        // 实际管理 NavPoint 的列表
        private List<NavPoint> navPoints = new List<NavPoint>();

        private void OnEnable()
        {
            NavPoint.OnNavPointCreated += RegisterNavPoint;
            NavPoint.OnNavPointDestroyed += UnregisterNavPoint;
            NavPoint.OnNavPointMoved += UpdateNavPoint;

            RefreshAllNavPoints(); // 初始化查找现有的 NavPoints
        }

        private void OnDisable()
        {
            NavPoint.OnNavPointCreated -= RegisterNavPoint;
            NavPoint.OnNavPointDestroyed -= UnregisterNavPoint;
            NavPoint.OnNavPointMoved -= UpdateNavPoint;
        }

        private void RefreshAllNavPoints()
        {
            navPoints.Clear();
            foreach (var nav in FindObjectsOfType<NavPoint>())
            {
                RegisterNavPoint(nav);
            }
        }

        private void RegisterNavPoint(NavPoint nav)
        {
            if (!navPoints.Contains(nav))
            {
                navPoints.Add(nav);
            }
        }

        private void UnregisterNavPoint(NavPoint nav)
        {
            navPoints.Remove(nav);
        }

        private void UpdateNavPoint(NavPoint nav)
        {
            // 保持 navPoints 是最新位置的，只在未来需要排序或其他逻辑时使用
        }


        public List<NavPoint> GetNavPoints()
        {
            return navPoints;
        }

        public NavPoint GetClosestNavPoint(Vector3 position)
        {
            NavPoint closest = null;
            float minDist = float.MaxValue;

            foreach (var nav in navPoints)
            {
                if (nav == null) continue;
                float dist = Vector3.Distance(nav.transform.position, position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = nav;
                }
            }

            return closest;
        }



#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (alwaysDrawPath)
            {
                DrawPath();
            }
        }


        private void DrawPath()
        {
            if (navPoints == null || navPoints.Count == 0) return;

            for (int i = 0; i < navPoints.Count; i++)
            {
                var nav = navPoints[i];
                if (nav == null) continue;

                GUIStyle labelStyle = new GUIStyle
                {
                    fontSize = 30,
                    normal = { textColor = debugColour }
                };

                if (drawNumbers)
                    Handles.Label(nav.transform.position, i.ToString(), labelStyle);

                if (i >= 1 && navPoints[i - 1] != null)
                {
                    Gizmos.color = debugColour;
                    Gizmos.DrawLine(navPoints[i - 1].transform.position, nav.transform.position);
                }
            }

            // 绘制闭环
            if (drawAsLoop && navPoints.Count > 1)
            {
                Gizmos.color = debugColour;
                Gizmos.DrawLine(navPoints[navPoints.Count - 1].transform.position, navPoints[0].transform.position);
            }
        }
#endif
    }
}
