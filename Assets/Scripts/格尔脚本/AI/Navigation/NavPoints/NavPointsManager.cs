using System.Collections.Generic;
using UnityEngine;
using DataStructures.ViliWonka.KDTree;
namespace Helltal.Gelercat
{
    public class NavPointsManager : MonoBehaviour
    {
        [Header("KDTree 参数")]
        public int maxPointsPerLeaf = 16;

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
            navPoints = new List<NavPoint>(FindObjectsOfType<NavPoint>());
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
                kdTree = new KDTree(maxPointsPerLeaf);
                kdTree.Build(pointPositions);
            }
            else
                kdTree.Build(pointPositions, maxPointsPerLeaf);

            treeDirty = false;
        }

        #endregion

        #region 查询接口（对外封装）

        /// <summary>
        /// 返回在某个pos周围，所有导航点，按距离升序排列的NavPoint引用（全量有序列表）
        /// </summary>
        public List<NavPoint> GetNearestPointsList(Vector3 currentPos)
        {
            if (treeDirty || kdTree == null)
                RebuildTree();

            int k = navPoints.Count; // 获取所有点并按距离排序
            queryResults.Clear();
            query.KNearest(kdTree, currentPos, k, queryResults);

            // 直接按index转NavPoint
            var result = new List<NavPoint>(k);
            foreach (int idx in queryResults)
            {
                if ((uint)idx < (uint)navPoints.Count)
                    result.Add(navPoints[idx]);
#if UNITY_EDITOR
                else
                    Debug.LogWarning($"KDTree returned out-of-range index: {idx}");
#endif
            }
            return result;
        }

        /// <summary>
        /// 返回在某个pos周围，距离在dist之内的navpoints集合
        /// </summary>
        public List<NavPoint> GetNearPointsBydist(Vector3 currentPos, float dist)
        {
            if (treeDirty || kdTree == null)
                RebuildTree();

            queryResults.Clear();
            query.Radius(kdTree, currentPos, dist, queryResults);

            var result = new List<NavPoint>(queryResults.Count);
            foreach (int idx in queryResults)
            {
                if ((uint)idx < (uint)navPoints.Count)
                    result.Add(navPoints[idx]);
#if UNITY_EDITOR
                else
                    Debug.LogWarning($"KDTree returned out-of-range index: {idx}");
#endif
            }
            return result;
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
