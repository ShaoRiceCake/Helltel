using UnityEngine;
using NPBehave;
using System.Collections.Generic;
using UnityEngine.AI;

namespace Helltal.Gelercat
{
    public class testMonsterController : GuestBase
    {
        [Header("伤害值")] public float baseDamage = 5f;
        [Header("等待时间")] public float waitingTimer = 180f;
        [Header("开心时间")] public float happyTimer = 3f;
        [Header("黑雾生成间隔时间")] public float duration = 0.1f;
        [Header("黑雾基础半径")] public float baseRadius = 0.1f;
        [Header("黑雾增加半径")] public float increaseRadius = 0.1f;

        public GameObject blackFog;
        public GameObject moneybag;

        private Root behaviorTree;
        private NavPoint currentTarget;

        private Dictionary<Transform, bool> visitDict = new Dictionary<Transform, bool>();

        protected override void Start()
        {
            base.Start();

            InitializeVisitDict();

            behaviorTree = CreateBehaviorTree();
            behaviorTree.Start();
        }

        private void InitializeVisitDict()
        {
            visitDict.Clear();
            foreach (var wp in navPointsManager.GetNavPoints())
            {
                if (wp?.GetTransform() != null && !visitDict.ContainsKey(wp.GetTransform()))
                {
                    visitDict.Add(wp.GetTransform(), false);
                }
            }
        }

        private Root CreateBehaviorTree()
        {
            return new Root(
                new Service(0.5f, UpdateTarget,
                    new Action(MoveToTarget)
                )
            );
        }

        private void UpdateTarget()
        {
            // 如果没有目标，或已经到达目标
            if (currentTarget == null ||
                visitDict.TryGetValue(currentTarget.GetTransform(), out bool visited) && visited ||
                Vector3.Distance(transform.position, currentTarget.transform.position) < 0.1f)
            {
                currentTarget = FindNextReachableWaypoint();

                if (currentTarget == null)
                {
                    ResetVisitStatus();
                    currentTarget = FindNextReachableWaypoint();
                }

                if (currentTarget != null)
                {
                    visitDict[currentTarget.GetTransform()] = true;
                }
            }
        }

        private void MoveToTarget()
        {
            if (currentTarget != null)
            {
                NegativeTo(currentTarget.GetTransform().position);
            }
        }

        private NavPoint FindNextReachableWaypoint()
        {
            NavPoint best = null;
            float shortest = float.MaxValue;

            foreach (var wp in navPointsManager.GetNavPoints())
            {
                if (wp.GetTransform() == null || visitDict.TryGetValue(wp.GetTransform(), out bool visited) && visited)
                    continue;

                if (!IsReachable(wp.GetTransform().position))
                {
                    // 如果不可达，标记为已访问
                    visitDict[wp.GetTransform()] = true; // 不可达也算访问了
                    continue;
                }

                float dist = Vector3.Distance(transform.position, wp.GetTransform().position);
                if (dist < shortest)
                {
                    shortest = dist;
                    best = wp;
                }
            }

            return best;
        }

        private void ResetVisitStatus()
        {
            Debug.Log("Reset Visit Status");
            List<Transform> keys = new List<Transform>(visitDict.Keys);
            foreach (var key in keys)
            {
                visitDict[key] = false;
            }
        }

        private bool IsReachable(Vector3 target)
        {
            NavMeshPath navPath = new NavMeshPath();
            return NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, navPath) &&
                   navPath.status == NavMeshPathStatus.PathComplete;
        }

    }
}
