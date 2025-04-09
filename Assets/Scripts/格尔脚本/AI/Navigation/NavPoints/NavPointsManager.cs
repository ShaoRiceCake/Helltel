// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;

// namespace Helltal.Gelercat
// {
//     [ExecuteAlways]
//     public class NavPointsManager : MonoBehaviour
//     {
//         [SerializeField]
//         private bool alwaysDrawPath = true;

//         [SerializeField]
//         private bool drawAsLoop = false;

//         [SerializeField]
//         private bool drawNumbers = true;

//         public Color debugColour = Color.white;

//         // ʵ�ʹ��� NavPoint ���б�
//         private List<NavPoint> navPoints = new List<NavPoint>();

//         private void OnEnable()
//         {
//             NavPoint.OnNavPointCreated += RegisterNavPoint;
//             NavPoint.OnNavPointDestroyed += UnregisterNavPoint;
//             NavPoint.OnNavPointMoved += UpdateNavPoint;

//             RefreshAllNavPoints(); // ��ʼ���������е� NavPoints
//         }

//         private void OnDisable()
//         {
//             NavPoint.OnNavPointCreated -= RegisterNavPoint;
//             NavPoint.OnNavPointDestroyed -= UnregisterNavPoint;
//             NavPoint.OnNavPointMoved -= UpdateNavPoint;
//         }

//         private void RefreshAllNavPoints()
//         {
//             navPoints.Clear();
//             foreach (var nav in FindObjectsOfType<NavPoint>())
//             {
//                 RegisterNavPoint(nav);
//             }
//         }

//         private void RegisterNavPoint(NavPoint nav)
//         {
//             if (!navPoints.Contains(nav))
//             {
//                 navPoints.Add(nav);
//             }
//         }

//         private void UnregisterNavPoint(NavPoint nav)
//         {
//             navPoints.Remove(nav);
//         }

//         private void UpdateNavPoint(NavPoint nav)
//         {
//             // ���� navPoints ������λ�õģ�ֻ��δ����Ҫ����������߼�ʱʹ��
//         }


//         public List<NavPoint> GetNavPoints()
//         {
//             return navPoints;
//         }

//         /// <summary>
//         /// ��ȡ��ָ��λ������� NavPoint
//         /// </summary>
//         /// <param name="position"> Ŀ��� </param>
//         /// <returns></returns>
//         /// <remarks> �÷������������ NavPoint�����ܽϵͣ�����ʹ�� </remarks>
//         public NavPoint GetClosestNavPoint(Vector3 position)
//         {
//             NavPoint closest = null;
//             float minDist = float.MaxValue;

//             foreach (var nav in navPoints)
//             {
//                 if (nav == null) continue;
//                 float dist = Vector3.Distance(nav.transform.position, position);
//                 if (dist < minDist)
//                 {
//                     minDist = dist;
//                     closest = nav;
//                 }
//             }

//             return closest;
//         }




// #if UNITY_EDITOR
//         private void OnDrawGizmos()
//         {
//             if (alwaysDrawPath)
//             {
//                 DrawPath();
//             }
//         }


//         private void DrawPath()
//         {
//             if (navPoints == null || navPoints.Count == 0) return;

//             for (int i = 0; i < navPoints.Count; i++)
//             {
//                 var nav = navPoints[i];
//                 if (nav == null) continue;

//                 GUIStyle labelStyle = new GUIStyle
//                 {
//                     fontSize = 30,
//                     normal = { textColor = debugColour }
//                 };

//                 if (drawNumbers)
//                     Handles.Label(nav.transform.position, i.ToString(), labelStyle);

//                 if (i >= 1 && navPoints[i - 1] != null)
//                 {
//                     Gizmos.color = debugColour;
//                     Gizmos.DrawLine(navPoints[i - 1].transform.position, nav.transform.position);
//                 }
//             }

//             // ���Ʊջ�
//             if (drawAsLoop && navPoints.Count > 1)
//             {
//                 Gizmos.color = debugColour;
//                 Gizmos.DrawLine(navPoints[navPoints.Count - 1].transform.position, navPoints[0].transform.position);
//             }
//         }
// #endif
//     }
// }


// �Ż�����
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

        // ʵ�ʹ��� NavPoint ���б�
        private List<NavPoint> navPoints = new List<NavPoint>();


        /// <summary>
        /// �������棬���� NavPoint ��λ�û��ֵ��������ڿ��ٲ��� NavPoints
        /// </summary>
        [SerializeField] private float gridCellSize = 10f;


        // ���� BoundingBox ��������
        private Dictionary<Vector2Int, List<NavPoint>> navGrid = new Dictionary<Vector2Int, List<NavPoint>>();

        private void OnEnable()
        {
            NavPoint.OnNavPointCreated += RegisterNavPoint;
            NavPoint.OnNavPointDestroyed += UnregisterNavPoint;
            NavPoint.OnNavPointMoved += UpdateNavPoint;

            RefreshAllNavPoints(); // ��ʼ���������е� NavPoints
        }

        private void OnDisable()
        {
            NavPoint.OnNavPointCreated -= RegisterNavPoint;
            NavPoint.OnNavPointDestroyed -= UnregisterNavPoint;
            NavPoint.OnNavPointMoved -= UpdateNavPoint;
        }

        public void RefreshAllNavPoints()
        {
            navPoints.Clear();
            navGrid.Clear(); // ������񻺴�
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
                Vector2Int key = GetGridKey(nav.transform.position);

                if (!navGrid.ContainsKey(key))
                {
                    navGrid[key] = new List<NavPoint>();
                }
                navGrid[key].Add(nav);
            }
        }

        private void UnregisterNavPoint(NavPoint nav)
        {
            if (!navPoints.Contains(nav)) return;
            Vector2Int key = GetGridKey(nav.transform.position);
            if (navGrid.ContainsKey(key))
            {
                navGrid[key].Remove(nav);
                if (navGrid[key].Count == 0)
                {
                    navGrid.Remove(key); // ����յ�����
                }
            }
            // �����б����Ƴ�
            navPoints.Remove(nav);

        }

        private void UpdateNavPoint(NavPoint nav)
        {
            // ���� navPoints ������λ�õģ�ֻ��δ����Ҫ����������߼�ʱʹ��
        }

        private Vector2Int GetGridKey(Vector3 position)
        {
            return new Vector2Int(
                Mathf.FloorToInt(position.x / gridCellSize),
                Mathf.FloorToInt(position.z / gridCellSize)  // ע��ʹ�� x-z ƽ��
            );
        }

        public List<NavPoint> GetNavPoints()
        {
            return navPoints;
        }

        /// <summary>
        /// ��ȡ��ָ��λ������� NavPoint
        /// </summary>
        /// <param name="position"> Ŀ��� </param>
        /// <returns></returns>
        /// <remarks>  </remarks>

        public NavPoint GetClosestNavPoint(Vector3 position)
        {
            Vector2Int key = GetGridKey(position);
            if (!navGrid.ContainsKey(key)) return null;

            NavPoint closest = null;
            float minSqrDist = float.MaxValue;

            foreach (var nav in navGrid[key])
            {
                if (nav == null) continue;

                float sqrDist = (nav.transform.position - position).sqrMagnitude;
                if (sqrDist < minSqrDist)
                {
                    minSqrDist = sqrDist;
                    closest = nav;
                }
            }

            return closest;
        }

        private List<NavPoint> GetNearbyNavPoints(Vector3 position)
        {
            Vector2Int center = GetGridKey(position);
            List<NavPoint> result = new List<NavPoint>();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    Vector2Int key = new Vector2Int(center.x + dx, center.y + dz);
                    if (navGrid.TryGetValue(key, out var list))
                    {
                        result.AddRange(list);
                    }
                }
            }

            return result;
        }




#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (alwaysDrawPath)
            {
                DrawPath();
            }
            DrawGridGizmos();
        }

        private void DrawPath()
        {
            if (navPoints == null || navPoints.Count == 0) return;

            for (int i = navPoints.Count - 1; i >= 0; i--)
            {
                var nav = navPoints[i];

                if (nav == null)
                {
                    // ���б����Ƴ������ٵ� NavPoint�������б�ɾ�
                    navPoints.RemoveAt(i);
                    continue;
                }

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

            // �ջ��߻���
            if (drawAsLoop && navPoints.Count > 1)
            {
                if (navPoints[navPoints.Count - 1] != null && navPoints[0] != null)
                {
                    Gizmos.color = debugColour;
                    Gizmos.DrawLine(navPoints[navPoints.Count - 1].transform.position, navPoints[0].transform.position);
                }
            }
        }


        private void DrawGridGizmos()
        {
            if (navPoints == null || navPoints.Count == 0) return;

            HashSet<Vector2Int> drawnCells = new HashSet<Vector2Int>();

            Gizmos.color = new Color(debugColour.r, debugColour.g, debugColour.b, 0.3f); // ��͸��

            foreach (var nav in navPoints)
            {
                if (nav == null) continue;

                Vector2Int key = GetGridKey(nav.transform.position);
                if (drawnCells.Contains(key)) continue; // ���ظ���
                drawnCells.Add(key);

                Vector3 center = new Vector3(
                    (key.x + 0.5f) * gridCellSize,
                    nav.transform.position.y,  // ������ NavPoint �߶�
                    (key.y + 0.5f) * gridCellSize
                );

                Vector3 size = new Vector3(gridCellSize, 0.1f, gridCellSize); // ��ƽ������
                Gizmos.DrawWireCube(center, size);

                if (drawNumbers)
                {
                    GUIStyle labelStyle = new GUIStyle
                    {
                        fontSize = 15,
                        normal = { textColor = debugColour }
                    };

                    Handles.Label(center + Vector3.up * 0.5f, $"({key.x}, {key.y})", labelStyle);
                }
            }
        }

#endif
    }
}
