using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DataStructures.ViliWonka.KDTree;
using System.Linq;


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

//         // 实际管理 NavPoint 的列表
//         private List<NavPoint> navPoints = new List<NavPoint>();


//         /// <summary>
//         /// 二级缓存，基于 NavPoint 的位置划分的网格，用于快速查找 NavPoints
//         /// </summary>
//         [SerializeField] private float gridCellSize = 10f;


//         // 基于 BoundingBox 二级缓存
//         private Dictionary<Vector2Int, List<NavPoint>> navGrid = new Dictionary<Vector2Int, List<NavPoint>>();

//         private void OnEnable()
//         {
//             NavPoint.OnNavPointCreated += RegisterNavPoint;
//             NavPoint.OnNavPointDestroyed += UnregisterNavPoint;
//             NavPoint.OnNavPointMoved += UpdateNavPoint;

//             RefreshAllNavPoints(); // 初始化查找现有的 NavPoints
//         }

//         private void OnDisable()
//         {
//             NavPoint.OnNavPointCreated -= RegisterNavPoint;
//             NavPoint.OnNavPointDestroyed -= UnregisterNavPoint;
//             NavPoint.OnNavPointMoved -= UpdateNavPoint;
//         }

//         public void RefreshAllNavPoints()
//         {
//             navPoints.Clear();
//             navGrid.Clear(); // 清空网格缓存
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
//                 Vector2Int key = GetGridKey(nav.transform.position);

//                 if (!navGrid.ContainsKey(key))
//                 {
//                     navGrid[key] = new List<NavPoint>();
//                 }
//                 navGrid[key].Add(nav);
//             }
//         }

//         private void UnregisterNavPoint(NavPoint nav)
//         {
//             if (!navPoints.Contains(nav)) return;
//             Vector2Int key = GetGridKey(nav.transform.position);
//             if (navGrid.ContainsKey(key))
//             {
//                 navGrid[key].Remove(nav);
//                 if (navGrid[key].Count == 0)
//                 {
//                     navGrid.Remove(key); // 清理空的网格
//                 }
//             }
//             // 从主列表中移除
//             navPoints.Remove(nav);

//         }

//         private void UpdateNavPoint(NavPoint nav)
//         {
//             // 保持 navPoints 是最新位置的，只在未来需要排序或其他逻辑时使用
//         }

//         private Vector2Int GetGridKey(Vector3 position)
//         {
//             return new Vector2Int(
//                 Mathf.FloorToInt(position.x / gridCellSize),
//                 Mathf.FloorToInt(position.z / gridCellSize)  // 注意使用 x-z 平面
//             );
//         }

//         public List<NavPoint> GetNavPoints()
//         {
//             return navPoints;
//         }

//         /// <summary>
//         /// 获取离指定位置最近的 NavPoint
//         /// </summary>
//         /// <param name="position"> 目标点 </param>
//         /// <returns></returns>
//         /// <remarks>  </remarks>

//         public NavPoint GetClosestNavPoint(Vector3 position)
//         {
//             Vector2Int key = GetGridKey(position);
//             if (!navGrid.ContainsKey(key)) return null;

//             NavPoint closest = null;
//             float minSqrDist = float.MaxValue;

//             foreach (var nav in navGrid[key])
//             {
//                 if (nav == null) continue;

//                 float sqrDist = (nav.transform.position - position).sqrMagnitude;
//                 if (sqrDist < minSqrDist)
//                 {
//                     minSqrDist = sqrDist;
//                     closest = nav;
//                 }
//             }

//             return closest;
//         }

//         private List<NavPoint> GetNearbyNavPoints(Vector3 position)
//         {
//             Vector2Int center = GetGridKey(position);
//             List<NavPoint> result = new List<NavPoint>();

//             for (int dx = -1; dx <= 1; dx++)
//             {
//                 for (int dz = -1; dz <= 1; dz++)
//                 {
//                     Vector2Int key = new Vector2Int(center.x + dx, center.y + dz);
//                     if (navGrid.TryGetValue(key, out var list))
//                     {
//                         result.AddRange(list);
//                     }
//                 }
//             }

//             return result;
//         }




// #if UNITY_EDITOR
//         private void OnDrawGizmos()
//         {
//             if (alwaysDrawPath)
//             {
//                 DrawPath();
//             }
//             DrawGridGizmos();
//         }

//         private void DrawPath()
//         {
//             if (navPoints == null || navPoints.Count == 0) return;

//             for (int i = navPoints.Count - 1; i >= 0; i--)
//             {
//                 var nav = navPoints[i];

//                 if (nav == null)
//                 {
//                     // 从列表中移除已销毁的 NavPoint，保持列表干净
//                     navPoints.RemoveAt(i);
//                     continue;
//                 }

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

//             // 闭环线绘制
//             if (drawAsLoop && navPoints.Count > 1)
//             {
//                 if (navPoints[navPoints.Count - 1] != null && navPoints[0] != null)
//                 {
//                     Gizmos.color = debugColour;
//                     Gizmos.DrawLine(navPoints[navPoints.Count - 1].transform.position, navPoints[0].transform.position);
//                 }
//             }
//         }


//         private void DrawGridGizmos()
//         {
//             if (navPoints == null || navPoints.Count == 0) return;

//             HashSet<Vector2Int> drawnCells = new HashSet<Vector2Int>();

//             Gizmos.color = new Color(debugColour.r, debugColour.g, debugColour.b, 0.3f); // 半透明

//             foreach (var nav in navPoints)
//             {
//                 if (nav == null) continue;

//                 Vector2Int key = GetGridKey(nav.transform.position);
//                 if (drawnCells.Contains(key)) continue; // 不重复画
//                 drawnCells.Add(key);

//                 Vector3 center = new Vector3(
//                     (key.x + 0.5f) * gridCellSize,
//                     nav.transform.position.y,  // 保持在 NavPoint 高度
//                     (key.y + 0.5f) * gridCellSize
//                 );

//                 Vector3 size = new Vector3(gridCellSize, 0.1f, gridCellSize); // 扁平立方体
//                 Gizmos.DrawWireCube(center, size);

//                 if (drawNumbers)
//                 {
//                     GUIStyle labelStyle = new GUIStyle
//                     {
//                         fontSize = 15,
//                         normal = { textColor = debugColour }
//                     };

//                     Handles.Label(center + Vector3.up * 0.5f, $"({key.x}, {key.y})", labelStyle);
//                 }
//             }
//         }

// #endif
//     }
// }





// v3.0 基于kdTree 的 NavPointsManager



namespace Helltal.Gelercat
{
    public class NavPointsManager : MonoBehaviour
    {
        [Header("KDTree 参数")]
        public int maxPointsPerLeaf = 16;
        public int kNearest = 5;

        private List<NavPoint> navPoints = new();
        private List<Vector3> pointPositions = new();

        private KDTree kdTree;
        private KDQuery query = new KDQuery();
        private List<int> queryResults = new();

        private bool treeDirty = false;

        #region Unity Lifecycle

        private void OnEnable()
        {
            NavPoint.OnNavPointCreated += RegisterNavPoint;
            NavPoint.OnNavPointDestroyed += UnregisterNavPoint;
            NavPoint.OnNavPointMoved += MarkTreeDirty;

            RefreshAllNavPoints();
        }

        private void OnDisable()
        {
            NavPoint.OnNavPointCreated -= RegisterNavPoint;
            NavPoint.OnNavPointDestroyed -= UnregisterNavPoint;
            NavPoint.OnNavPointMoved -= MarkTreeDirty;
        }

        #endregion

        #region NavPoint 注册

        private void RegisterNavPoint(NavPoint point)
        {
            if (!navPoints.Contains(point))
            {
                navPoints.Add(point);
                treeDirty = true;
            }
        }

        private void UnregisterNavPoint(NavPoint point)
        {
            if (navPoints.Remove(point))
                treeDirty = true;
        }

        private void MarkTreeDirty(NavPoint point)
        {
            treeDirty = true;
        }

        private void RefreshAllNavPoints()
        {
            navPoints = FindObjectsOfType<NavPoint>().ToList();
            treeDirty = true;
            RebuildTree();
        }

        #endregion

        #region KDTree 构建

        private void RebuildTree()
        {
            pointPositions = new List<Vector3>(navPoints.Count);
            foreach (var point in navPoints)
                pointPositions.Add(point.transform.position);

            if (kdTree == null)
            {
                kdTree = new KDTree(32);
                kdTree.Build(pointPositions);
            }
            else
                kdTree.Build(pointPositions, maxPointsPerLeaf);

            treeDirty = false;
        }

        #endregion

        #region 查询接口

        /// <summary>
        /// 返回当前位置最近的 NavPoint 列表（按距离升序）
        /// </summary>
        public List<NavPoint> GetNearestPoints(Vector3 currentPos, int k = 5)
        {
            if (treeDirty || kdTree == null)
                RebuildTree();
            k = navPoints.Count; // return all points
            queryResults.Clear();
            query.KNearest(kdTree, currentPos, k, queryResults);

            var result = new List<NavPoint>(k);
            foreach (int i in queryResults)
            {
                if ((uint)i < (uint)navPoints.Count)
                    result.Add(navPoints[i]);
#if UNITY_EDITOR
                else
                    Debug.LogWarning($"KDTree returned out-of-range index: {i}");
#endif
            }
            return result;
        }

        /// <summary>
        /// 返回最靠近当前位置、满足条件的第一个合法 NavPoint
        /// </summary>
        public NavPoint GetNearestValidPoint(Vector3 currentPos, Predicate<NavPoint> filter)
        {
            var candidates = GetNearestPoints(currentPos, kNearest);
            foreach (var point in candidates)
            {
                if (filter == null || filter(point))
                    return point;
            }
            return null;
        }
        
        #endregion
        public List<NavPoint> GetNavPoints()
        {
            return navPoints;
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (queryResults != null && pointPositions != null)
            {
                Gizmos.color = Color.green;
                foreach (int i in queryResults)
                {
                    if ((uint)i < (uint)pointPositions.Count)
                        Gizmos.DrawWireSphere(pointPositions[i], 0.3f);
                }
            }
        }
#endif
    }
}
