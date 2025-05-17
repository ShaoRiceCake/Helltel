using UnityEngine;
using UnityEngine.AI;
using NPBehave;
using System.Collections.Generic;

namespace Helltal.Gelercat
{
    public class Patrol : Task
    {
        private NavMeshAgent agent;
        private NavPointsManager navPointsManager;
        private Dictionary<NavPoint, bool> visitDict;
        private NavPoint currentTarget;

        private float searchDist; // 搜索距离

        public Patrol(NavMeshAgent agent, NavPointsManager navPointsManager, float SerchingDist = 1.5f) : base("Patrol")
        {
            this.agent = agent;
            this.navPointsManager = navPointsManager;
            this.searchDist = SerchingDist;
            visitDict = new Dictionary<NavPoint, bool>();
        }

        protected override void DoStart()
        {

            if (agent == null || agent.gameObject == null || !agent.isOnNavMesh)
            {
                Debug.LogWarning("Patrol agent not ready or not on NavMesh.");
                this.Stopped(false);
                return;
            }

            ChooseNextNavPoint();

        }

        protected override void DoStop()
        {
            // agent.ResetPath();  // 停止导航
            // this.Stopped(false); // 明确停止
            if (agent != null && agent.isOnNavMesh)
            {
                agent.ResetPath();
            }
            Clock.RemoveUpdateObserver(OnUpdate);  // <<< 关键，移除更新回调！！
            this.Stopped(false); // 明确告诉父节点：自己停止了
        }

        
        private void ChooseNextNavPoint()
        {
            if (agent == null || agent.gameObject == null) return;

            int attempts = 0;
            int maxAttempts = 2; // 防止死循环

            while (attempts < maxAttempts)
            {
                currentTarget = GetClosestUnvisitedNavPoint();

                if (currentTarget == null)
                {
                    visitDict.Clear();  // 重置访问状态
                    currentTarget = GetClosestUnvisitedNavPoint();

                    if (currentTarget == null)
                    {
                        Debug.Log("无任何可达导航点，Patrol失败");
                        this.Stopped(false);
                        return;
                    }
                }

                float distToTarget = Vector3.Distance(agent.transform.position, currentTarget.transform.position);
                if (distToTarget <= searchDist)
                {
                    visitDict[currentTarget] = true;
                    Debug.Log("导航点在搜索距离内，跳过：" + currentTarget.name);
                    attempts++;
                    continue;
                }

                if (!agent.SetDestination(currentTarget.transform.position))
                {
                    visitDict[currentTarget] = true;
                    Debug.Log("无法到达导航点：" + currentTarget.name);
                    attempts++;
                    continue;
                }

                Clock.AddUpdateObserver(OnUpdate);
                return; // 设置成功，退出
            }

            Debug.LogWarning("超过最大尝试次数，Patrol失败");
            this.Stopped(false);
        }

        private void OnUpdate()
        {
            if (ReachedTarget(currentTarget))
            {
                visitDict[currentTarget] = true;
                Clock.RemoveUpdateObserver(OnUpdate);
                ChooseNextNavPoint();  // 自动选择下一个点继续巡逻
            }
        }

        private NavPoint GetClosestUnvisitedNavPoint()
        {

            NavPoint closest = null;
            float minDist = float.MaxValue;
            Vector3 pos = agent.transform.position;

            foreach (var nav in navPointsManager.GetNavPoints())
            {
                if (nav == null || visitDict.ContainsKey(nav)) continue;

                NavMeshPath path = new NavMeshPath();
                if (!agent.CalculatePath(nav.transform.position, path) || path.status != NavMeshPathStatus.PathComplete)
                    continue;

                float dist = (nav.transform.position - pos).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = nav;
                }
            }
            return closest;
        }

        private bool ReachedTarget(NavPoint target)
        {
            // return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath;
            if (target == null || agent == null) return false;

            float distance = Vector3.Distance(agent.transform.position, target.transform.position);
            return distance <= searchDist;
        }
    }
}
