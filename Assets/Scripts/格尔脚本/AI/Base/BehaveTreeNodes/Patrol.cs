using System.Collections.Generic;
using Helltal.Gelercat;
using UnityEngine;
using UnityEngine.AI;

namespace NPBehave
{
    public class Patrol : Task
    {
        private NavMeshAgent agent;
        private NavPointsManager navPointsManager;
        private Dictionary<NavPoint, bool> visitDict;
        private NavPoint currentTarget;

        private float searchDist;

        public Patrol(NavMeshAgent agent, NavPointsManager navPointsManager, float searchDist = 1.5f) : base("Patrol")
        {
            this.agent = agent;
            this.navPointsManager = navPointsManager;
            this.searchDist = searchDist;
            this.visitDict = new Dictionary<NavPoint, bool>();
        }

        protected override void DoStart()
        {
            if (agent == null || navPointsManager == null)
            {
                Debug.LogWarning("Patrol: agent or navPointsManager is null!");
                this.Stopped(false);
                return;
            }
            TryChooseNextTarget();
            this.Clock.AddUpdateObserver(OnUpdate);
        }

        protected override void DoStop()
        {
            Debug.Log("Patrol: Stopped");
            this.Clock.RemoveUpdateObserver(OnUpdate);
            agent.ResetPath();
            this.Stopped(true);
        }

        private void OnUpdate()
        {
            // “顺路打卡”：所有范围内点都记已访问
            MarkNearbyPointsAsVisited();

            // 到达当前目标
            if (ReachedTarget(currentTarget))
            {
                TryChooseNextTarget();
            }
        }

        private void TryChooseNextTarget()
        {
            currentTarget = GetClosestUnvisitedNavPoint();
            if (currentTarget != null)
            {
                agent.SetDestination(currentTarget.transform.position);
            }
            else
            {
                this.Clock.RemoveUpdateObserver(OnUpdate);
                this.Stopped(true);
            }
        }

        private void MarkNearbyPointsAsVisited()
        {
            var points = navPointsManager.GetNearPointsBydist(agent.transform.position, searchDist);
            foreach (var nav in points)
            {
                if (nav == null) continue;
                if (!visitDict.ContainsKey(nav) &&
                    Vector3.Distance(agent.transform.position, nav.transform.position) <= searchDist)
                {
                    visitDict[nav] = true;
                }
            }
        }

        private bool ReachedTarget(NavPoint target)
        {
            if (target == null || agent == null || agent.pathPending) return false;
            return Vector3.Distance(agent.transform.position, target.transform.position) <= searchDist;
        }

        private NavPoint GetClosestUnvisitedNavPoint()
        {
            Vector3 pos = agent.transform.position;
            var points = navPointsManager.GetNearestPointsList(pos);            foreach (var nav in points)
            {
                if (nav != null && !visitDict.ContainsKey(nav))
                    return nav;
            }
            // 全部访问过，清空再开始
            visitDict.Clear();
            return points.Count > 0 ? points[0] : null;
        }
    }
}
