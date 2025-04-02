using UnityEngine;
using NPBehave;

namespace Helltal.Gelercat
{
    public class OldManController : GuestBase
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
            // ���ﵱǰĿ����ʼ��Ŀ��ʱ���л�Ŀ��
            Debug.Log("UpdateTarget");
            if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.point.position) < 1f)
            {
                currentTarget = FindNearestUnvisitedWaypoint();
                if (currentTarget == null)
                {
                    path.ResetAllCheck();  // �������е�
                    currentTarget = FindNearestUnvisitedWaypoint();
                }

                currentTarget.Check(true); // ����ѷ���
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
