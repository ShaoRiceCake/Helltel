// using UnityEngine;
// using NPBehave;
// using System.Collections.Generic;
// using UnityEngine.AI;

// namespace Helltal.Gelercat
// {
//     public class TestMonsterController : GuestBase
//     {
//         [Header("�˺�ֵ")] public float baseDamage = 5f;
//         [Header("�ȴ�ʱ��")] public float waitingTimer = 180f;
//         [Header("����ʱ��")] public float happyTimer = 3f;
//         [Header("�������ɼ��ʱ��")] public float duration = 0.1f;
//         [Header("��������뾶")] public float baseRadius = 0.1f;
//         [Header("�������Ӱ뾶")] public float increaseRadius = 0.1f;

//         public GameObject blackFog;
//         public GameObject moneybag;

//         private Root behaviorTree;
//         private NavPoint currentTarget;
//         private Dictionary<Transform, bool> visitDict = new Dictionary<Transform, bool>();
//         private NavMeshPath cachedPath;
//         private Blackboard blackboard;
//         protected override void Start()
//         {
//             base.Start();
//             cachedPath = new NavMeshPath();


//             behaviorTree = CreateBehaviorTree();
//             blackboard = behaviorTree.Blackboard;
//             behaviorTree.Start();
//         }



//         private Root CreateBehaviorTree()
//         {

//             return new Root(
//                 new Service(0.5f, UpdateTargetLogic,
//                     new Selector(
//                         // �����Ŀ�꣬������Ŀ��
//                         new BlackboardCondition("HasTarget", Operator.IS_EQUAL, true, Stops.LOWER_PRIORITY_IMMEDIATE_RESTART,
//                             new Sequence(
//                                 new Action(() => MoveToTarget()),
//                                 new Condition(() => HasArrived(), Stops.SELF,
//                                     new WaitUntilStopped()
//                                 ),
//                                 new Action(() => OnReachTarget())
//                             )
//                         ),

//                         // ����������Ŀ��
//                         new Action(() => SetRandomTarget())
//                     )
//                 )
//             );

//         }

//         private void UpdateTargetLogic()
//         {
//             // �Զ����� visitDict ��Ϊ null �� entry
//             foreach (var key in new List<Transform>(visitDict.Keys))
//             {
//                 if (key == null)
//                     visitDict.Remove(key);
//             }



//             bool hasTarget = currentTarget != null;
//             if (blackboard.Get<bool>("HasTarget") != hasTarget)
//             {
//                 blackboard["HasTarget"] = hasTarget;
//             }

//         }

//         private void SetRandomTarget()
//         {
//             if (!agent.isOnNavMesh)
//             {
//                 Debug.LogWarning("Agent not on NavMesh yet, cannot calculate path.");
//                 return;
//             }

//             var navPoints = navPointsManager.GetNavPoints();
//             List<NavPoint> reachable = new List<NavPoint>();

//             foreach (var nav in navPoints)
//             {
//                 if (nav == null || (visitDict.ContainsKey(nav.transform) && visitDict[nav.transform])) continue;

//                 if (agent.CalculatePath(nav.transform.position, cachedPath) &&
//                     cachedPath.status == NavMeshPathStatus.PathComplete)
//                 {
//                     reachable.Add(nav);
//                 }
//             }

//             if (reachable.Count == 0)
//             {
//                 visitDict.Clear();
//                 blackboard["HasTarget"] = false;
//                 return;
//             }

//             currentTarget = reachable[UnityEngine.Random.Range(0, reachable.Count)];
//             blackboard["HasTarget"] = true;
//         }


//         private void MoveToTarget()
//         {
//             if (currentTarget != null)
//             {
//                 agent.SetDestination(currentTarget.transform.position);
//             }
//         }

//         private bool HasArrived()
//         {
//             if (currentTarget == null) return false;

//             float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
//             return distance < agent.stoppingDistance + 0.2f && !agent.pathPending;
//         }

//         private void OnReachTarget()
//         {
//             if (currentTarget != null)
//             {
//                 visitDict[currentTarget.transform] = true;
//                 currentTarget = null;
//                 blackboard["HasTarget"] = false;
//                 blackboard["IsMoving"] = false;
//             }
//         }



//     }
// }


using UnityEngine;
using NPBehave;
using System.Collections.Generic;
using UnityEngine.AI;

namespace Helltal.Gelercat
{
    public class TestMonsterController : GuestBase
    {
        [Header("�˺�ֵ")] public float baseDamage = 5f;
        [Header("�ȴ�ʱ��")] public float waitingTimer = 180f;
        [Header("����ʱ��")] public float happyTimer = 3f;
        [Header("�������ɼ��ʱ��")] public float duration = 0.1f;
        [Header("��������뾶")] public float baseRadius = 0.1f;
        [Header("�������Ӱ뾶")] public float increaseRadius = 0.1f;

        public GameObject blackFog;
        public GameObject moneybag;

        private Root behaviorTree;
        private NavPoint currentTarget;
        private Dictionary<Transform, bool> visitDict = new Dictionary<Transform, bool>();
        private NavMeshPath cachedPath;

        private enum PatrolState { Idle, Moving }
        private PatrolState patrolState = PatrolState.Idle;

        protected override void Start()
        {
            base.Start();
            cachedPath = new NavMeshPath();
            behaviorTree = CreateBehaviorTree();
            behaviorTree.Start();
        }

        private Root CreateBehaviorTree()
        {
            return new Root(
                new Service(0.5f, UpdateTargetLogic,
                    new Selector(
                        new Condition(() => patrolState == PatrolState.Moving, Stops.SELF,
                            new Sequence(
                                new Action(() => MoveToTarget()),
                                new Condition(() => HasArrived(), Stops.SELF,
                                    new WaitUntilStopped()
                                ),
                                new Action(() => OnReachTarget())
                            )
                        ),
                        new Condition(() => patrolState == PatrolState.Idle, Stops.SELF,
                            new Action(() => SetRandomTarget())
                        )
                    )
                )
            );
        }

        void Update()
        {
            Debug.Log("Current Patrol State: " + patrolState);
        }
        private void UpdateTargetLogic()
        {
            // �Զ�����ʧЧĿ��
            foreach (var key in new List<Transform>(visitDict.Keys))
            {
                if (key == null)
                    visitDict.Remove(key);
            }
        }

        private void SetRandomTarget()
        {
            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning("Agent not on NavMesh yet.");
                return;
            }

            var navPoints = navPointsManager.GetNavPoints();
            List<NavPoint> reachable = new List<NavPoint>();

            foreach (var nav in navPoints)
            {
                if (nav == null) continue;

                bool alreadyVisited = visitDict.ContainsKey(nav.transform) && visitDict[nav.transform];
                if (alreadyVisited) continue;

                if (agent.CalculatePath(nav.transform.position, cachedPath) &&
                    cachedPath.status == NavMeshPathStatus.PathComplete)
                {
                    reachable.Add(nav);
                }
            }

            // ����δ���ʵ㣬���÷���״̬
            if (reachable.Count == 0)
            {
                visitDict.Clear();
                patrolState = PatrolState.Idle;
                return;
            }

            currentTarget = reachable[UnityEngine.Random.Range(0, reachable.Count)];
            patrolState = PatrolState.Moving;
        }

        private void MoveToTarget()
        {
            if (currentTarget != null)
            {
                agent.SetDestination(currentTarget.transform.position);
            }
        }


private bool HasArrived()
{
    if (currentTarget == null) return false;

    Vector3 posA = transform.position;
    Vector3 posB = currentTarget.transform.position;

    float horizontalDistance = Vector2.Distance(new Vector2(posA.x, posA.z), new Vector2(posB.x, posB.z));
    bool closeEnough = horizontalDistance <= agent.stoppingDistance + 0.2f;

    // �ж��Ƿ񼸺���ֹ
    bool notMoving = agent.velocity.sqrMagnitude < 0.05f;

    // NavMesh ����׼��·��ʱ����
    bool notPending = !agent.pathPending;

    bool arrived = closeEnough && notPending && notMoving;

    // ��ѡ��ӡ����
    // Debug.Log($"Arrived? H-Dist: {horizontalDistance:F2}, Close: {closeEnough}, Vel: {agent.velocity.magnitude:F2}, Result: {arrived}");

    return arrived;
}




        private void OnReachTarget()
        {
            if (currentTarget != null)
            {
                visitDict[currentTarget.transform] = true;
                currentTarget = null;
            }
            patrolState = PatrolState.Idle;
        }
    }
}
