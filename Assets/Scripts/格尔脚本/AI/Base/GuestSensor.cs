using System.Collections.Generic;
using UnityEngine;

namespace Helltal.Gelercat
{
    [ExecuteAlways]
    public class GuestSensor : MonoBehaviour
    {
        [Header("Sensor Settings")]
        public GameObject sensorSource; // �ɵ��ڵĴ�����Դ
        public float viewAngle = 60f;   // ��Ұ�Ƕȣ�׶�Σ�
        public float viewDistance = 10f; // ̽�����
        public LayerMask detectionLayer; // Ҫ��������㼶����ҡ�����ȣ�
        public bool isDebug = true; // �Ƿ����õ���ģʽ
        [Header("Scan Result")]
        public List<Transform> detectedTargets = new List<Transform>();

        private void Start()
        {
            EnsureSensorSourceExists();
        }

        private void Update()
        {
            Scan();
            if(isDebug && detectedTargets.Count > 0)
            {
                foreach (var target in detectedTargets)
                {
                    Debug.Log("Detected: " + target.name);
                }
            }
        }

        private void EnsureSensorSourceExists()
        {
            if (sensorSource == null)
            {
                sensorSource = new GameObject("SensorSource");
                sensorSource.transform.SetParent(transform);
                sensorSource.transform.localPosition = Vector3.zero; // �����߶�
            }
        }

        private void Scan()
        {
            detectedTargets.Clear();

            Collider[] hits = Physics.OverlapSphere(sensorSource.transform.position, viewDistance, detectionLayer);

            foreach (var hit in hits)
            {
                Vector3 dirToTarget = (hit.transform.position - sensorSource.transform.position).normalized;
                float angleToTarget = Vector3.Angle(sensorSource.transform.forward, dirToTarget);

                if (angleToTarget < viewAngle / 2f)
                {
                    detectedTargets.Add(hit.transform);
                }
            }
        }

        // ���ӻ�׶����Ұ
        private void OnDrawGizmos()
        {
            if (sensorSource == null) return;

            Vector3 origin = sensorSource.transform.position;
            Vector3 forward = sensorSource.transform.forward;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(origin, viewDistance);

            // �����α�Ե
            Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * forward;
            Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + left * viewDistance);
            Gizmos.DrawLine(origin, origin + right * viewDistance);

            // ��׶��������򣨿�ѡ��
            Gizmos.color = new Color(1, 1, 0, 0.1f);
            Gizmos.DrawRay(origin, left * viewDistance);
            Gizmos.DrawRay(origin, right * viewDistance);
        }
    }
}
