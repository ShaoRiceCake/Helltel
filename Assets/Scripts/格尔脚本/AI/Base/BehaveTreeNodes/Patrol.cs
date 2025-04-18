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

        public Patrol(NavMeshAgent agent, NavPointsManager navPointsManager) : base("Patrol")
        {
            this.agent = agent;
            this.navPointsManager = navPointsManager;
            visitDict = new Dictionary<NavPoint, bool>();
        }

        protected override void DoStart()
        {
            ChooseNextNavPoint();
        }

        protected override void DoStop()
        {
            agent.ResetPath();  // 停止导航
            this.Stopped(false); // 明确停止
        }

        private void ChooseNextNavPoint()
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

            bool setDestinationSuccess = agent.SetDestination(currentTarget.transform.position);
            if (!setDestinationSuccess)
            {
                visitDict[currentTarget] = true;
                Debug.Log("无法到达导航点：" + currentTarget.name);
                ChooseNextNavPoint(); // 尝试下一个点
                return;
            }

            Clock.AddUpdateObserver(OnUpdate);
        }

        private void OnUpdate()
        {
            if (ReachedTarget(currentTarget))
            {
                visitDict[currentTarget] = true;
                Debug.Log("到达导航点：" + currentTarget.name);

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
            return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath;
        }
    }
}
