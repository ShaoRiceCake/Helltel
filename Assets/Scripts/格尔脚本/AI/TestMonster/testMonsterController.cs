using UnityEngine;
using NPBehave;
using System.Collections.Generic;
using UnityEngine.AI;

namespace Helltal.Gelercat
{
    public class TestMonsterController : GuestBase
    {
        public GameObject blackFog;
        public GameObject moneybag;

        private Root behaviorTree;
        private NavPoint currentTarget;
        private Dictionary<NavPoint, bool> visitDict = new Dictionary<NavPoint, bool>();

        private enum PatrolState { Idle, Moving }
        private PatrolState patrolState = PatrolState.Idle;

        protected override void Start()
        {
            base.Start();
            behaviorTree = CreateBehaviorTree();
            behaviorTree.Start();
        }

        private Root CreateBehaviorTree()
        {
            return new Root(
                new Repeater(
                    new Sequence(
                        new Action(() => {
                            currentTarget = GetClosestUnvisitedNavPoint();
                            if (currentTarget == null)
                            {
                                Debug.Log("无未访问导航点，重置状态");
                                visitDict.Clear();
                                currentTarget = GetClosestUnvisitedNavPoint();
                                if (currentTarget == null)
                                {
                                    Debug.Log("重置后依然无可用导航点，AI进入Idle状态");
                                    return false;
                                }
                            }

                            if (!agent.SetDestination(currentTarget.transform.position))
                            {
                                Debug.Log("无法到达导航点: " + currentTarget.name);
                                visitDict[currentTarget] = true; // 跳过此点
                                return false;
                            }

                            patrolState = PatrolState.Moving;
                            return true;
                        }),
                        new WaitForCondition(() => ReachedTarget(currentTarget),
                            new Action(() => {
                                Debug.Log("到达目标点: " + currentTarget.name);
                                visitDict[currentTarget] = true;
                                patrolState = PatrolState.Idle;
                                return true;
                            })
                        )
                    )
                )
            );
        }

        private NavPoint GetClosestUnvisitedNavPoint()
        {
            NavPoint closest = null;
            float minDist = float.MaxValue;
            Vector3 pos = transform.position;

            foreach (var nav in navPointsManager.GetNavPoints())
            {
                if (nav == null || visitDict.ContainsKey(nav)) continue;

                NavMeshPath path = new NavMeshPath();
                if (!agent.CalculatePath(nav.transform.position, path) || path.status != NavMeshPathStatus.PathComplete)
                {
                    continue; // 无法到达则跳过
                }

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
            if (target == null || agent == null) return false;
            return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath;
        }

        protected override void Update()
        {
            base.Update();
            if (debugging)
                Debug.Log("Current Patrol State: " + patrolState);
        }
    }
}
