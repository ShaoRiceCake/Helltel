// using UnityEngine;
// using UnityEngine.AI;
// using NPBehave;
// using System.Collections.Generic;

// namespace Helltal.Gelercat
// {
//     public class Patrol : Task
//     {
//         private NavMeshAgent agent;
//         private NavPointsManager navPointsManager;
//         private Dictionary<NavPoint, bool> visitDict;
//         private NavPoint currentTarget;

//         private float searchDist; // 搜索距离

//         public Patrol(NavMeshAgent agent, NavPointsManager navPointsManager, float SerchingDist = 1.5f) : base("Patrol")
//         {
//             this.agent = agent;
//             this.navPointsManager = navPointsManager;
//             this.searchDist = SerchingDist;
//             visitDict = new Dictionary<NavPoint, bool>();
//         }

//         protected override void DoStart()
//         {

//             if (agent == null || agent.gameObject == null || !agent.isOnNavMesh)
//             {
//                 Debug.LogWarning("Patrol agent not ready or not on NavMesh.");
//                 this.Stopped(false);
//                 return;
//             }

//             ChooseNextNavPoint();

//         }

//         protected override void DoStop()
//         {
//             // agent.ResetPath();  // 停止导航
//             // this.Stopped(false); // 明确停止
//             if (agent != null && agent.isOnNavMesh)
//             {
//                 agent.ResetPath();
//             }
//             Clock.RemoveUpdateObserver(OnUpdate);  // <<< 关键，移除更新回调！！
//             this.Stopped(false); // 明确告诉父节点：自己停止了
//         }


//         private void ChooseNextNavPoint()
//         {
//             if (agent == null || agent.gameObject == null) return;

//             currentTarget = GetClosestUnvisitedNavPoint();
//             float distToTarget = Vector3.Distance(agent.transform.position, currentTarget.transform.position);
//             if (distToTarget <= searchDist)
//             {
//                 visitDict[currentTarget] = true;
//                 Debug.Log("导航点在搜索距离内，跳过：" + currentTarget.name);
//             }
//             if (!agent.SetDestination(currentTarget.transform.position))
//             {
//                 visitDict[currentTarget] = true;
//                 Debug.Log("无法到达导航点：" + currentTarget.name);
//             }
//             Clock.AddUpdateObserver(OnUpdate); // 添加更新回调
//             Debug.LogWarning("超过最大尝试次数，Patrol失败");
//             this.Stopped(false);
//         }

//         private void OnUpdate()
//         {
//             if (ReachedTarget(currentTarget))
//             {
//                 visitDict[currentTarget] = true;
//                 Clock.RemoveUpdateObserver(OnUpdate);
//                 ChooseNextNavPoint();  // 自动选择下一个点继续巡逻
//             }
//         }

//         private NavPoint GetClosestUnvisitedNavPoint()
//         {
//             Vector3 pos = agent.transform.position;
//             List<NavPoint> npl = navPointsManager.GetNearestPoints(pos);
//             foreach (var nav in npl)
//             {
//                 if (nav == null || visitDict.ContainsKey(nav)) continue; // 已经访问过的点
//                 else
//                 {

//                     return nav; // 直接返回第一个未访问的点
//                 }
//             }
//             // 清空访问字典
//             visitDict.Clear();
//             return npl[0]; // 返回第一个点
//         }

//         private bool ReachedTarget(NavPoint target)
//         {
//             // return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath;
//             if (target == null || agent == null) return false;

//             float distance = Vector3.Distance(agent.transform.position, target.transform.position);
//             return distance <= searchDist;
//         }
//     }
// }



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

        private float searchDist;
        private int maxAttempts = 5;


        public Patrol(NavMeshAgent agent, NavPointsManager navPointsManager, float searchDist = 1.5f) : base("Patrol")
        {
            this.agent = agent;
            this.navPointsManager = navPointsManager;
            this.searchDist = searchDist;
            this.visitDict = new Dictionary<NavPoint, bool>();
        }

        protected override void DoStart()
        {
            if (agent == null || !agent.isOnNavMesh)
            {
                Debug.LogWarning("Patrol failed: Agent invalid or not on NavMesh.");
                Stopped(false);
                return;
            }
            TryChooseNextTarget();
        }

        protected override void DoStop()
        {
            if (agent != null && agent.isOnNavMesh)
                agent.ResetPath();

            Clock.RemoveUpdateObserver(OnUpdate);
            currentTarget = null;
            Stopped(false);
        }

        private void TryChooseNextTarget()
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                currentTarget = GetClosestUnvisitedNavPoint();

                if (currentTarget == null)
                {
                    Debug.LogWarning("Patrol failed: 无可用目标");
                    Stopped(false);
                    return;
                }

                float distance = Vector3.Distance(agent.transform.position, currentTarget.transform.position);
                if (distance <= searchDist)
                {
                    Debug.Log("导航点太近，跳过: " + currentTarget.name);
                    continue;
                }

                if (!agent.SetDestination(currentTarget.transform.position))
                {
                    Debug.LogWarning("无法到达导航点: " + currentTarget.name);
                    continue;
                }

                // 成功设置目标
                Clock.AddUpdateObserver(OnUpdate);
                return;
            }

            Debug.LogWarning("Patrol failed: 超过最大尝试次数");
            Stopped(false);
        }

        private void OnUpdate()
        {
            if (ReachedTarget(currentTarget))
            {
                visitDict[currentTarget] = true;
                Clock.RemoveUpdateObserver(OnUpdate);
                TryChooseNextTarget();
            }
        }

        private NavPoint GetClosestUnvisitedNavPoint()
        {
            Vector3 pos = agent.transform.position;
            var points = navPointsManager.GetNearestPoints(pos, 10);

            foreach (var nav in points)
            {
                if (nav == null) continue;
                if (!visitDict.ContainsKey(nav)) return nav;
            }

            visitDict.Clear(); // 所有点访问完就重置
            return points.Count > 0 ? points[0] : null;
        }

        private bool ReachedTarget(NavPoint target)
        {
            if (target == null || agent == null || agent.pathPending) return false;
            return Vector3.Distance(agent.transform.position, target.transform.position) <= searchDist;
        }
    }
}
