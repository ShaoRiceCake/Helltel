using UnityEngine;
using NPBehave;

namespace Helltal.Gelercat
{
    public class OldManController : GuestBase
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
        private WayPoint currentTarget;

        public void Start()
        {
            base.Start();

        

            behaviorTree = CreateBehaviorTree();
            behaviorTree.Start();
        }

        private Root CreateBehaviorTree()
        {
            return new Root(
                new Service(0.2f, UpdateTarget,
                    new Action(MoveToTarget)
                )
            );
        }

        private void UpdateTarget()
        {
            // 到达当前目标或初始无目标时，切换目标
            Debug.Log("UpdateTarget");
            if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.point.position) < 1f)
            {
                currentTarget = FindNearestUnvisitedWaypoint();
                if (currentTarget == null)
                {
                    path.ResetAllCheck();  // 重置所有点
                    currentTarget = FindNearestUnvisitedWaypoint();
                }

                currentTarget.Check(true); // 标记已访问
            }
        }

        private void MoveToTarget()
        {
            Debug.Log("MoveToTarget");
            if (currentTarget != null)
            {
                NegativeTo(currentTarget.point.position);
            }
        }

        private WayPoint FindNearestUnvisitedWaypoint()
        {
            WayPoint nearest = null;
            float shortestDistance = float.MaxValue;

            foreach (var wp in path.waypoints)
            {
                if (!wp.isCheck)
                {
                    float distance = Vector3.Distance(transform.position, wp.point.position);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        nearest = wp;
                    }
                }
            }

            return nearest;
        }

        private void OnDestroy()
        {
            if (behaviorTree != null && behaviorTree.CurrentState == Node.State.ACTIVE)
            {
                behaviorTree.Stop();
            }
        }
    }
}
