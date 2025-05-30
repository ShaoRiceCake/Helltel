using System.Collections.Generic;
using UnityEngine;

namespace Helltal.Gelercat
{
    [ExecuteAlways]
    public class GuestSensor : MonoBehaviour
    {
        [Header("Sensor Settings")]
        public GameObject sensorSource; // 可调节的传感器源
        public float viewAngle = 60f;   // 视野角度（锥形）
        public float viewDistance = 10f; // 探测距离
        public LayerMask detectionLayer; // 要检测的生物层级（玩家、生物等）
        
        public LayerMask ObstructionLayer; // 遮挡层级（墙壁、地面等）
        
        public bool isDebug = true; // 是否启用调试模式
        [Header("Scan Result")]
        public List<Transform> detectedTargets;

        private const int ScanFrameInterval = 5;


        public void Awake()
        {
            detectedTargets = new List<Transform>();

        }
        private void Start()
        {
            EnsureSensorSourceExists();
        }

        private void Update()
        {
            if (Time.frameCount % ScanFrameInterval == 0) // 每3帧检测一次
            {
                Scan();
            }
        }
        private void EnsureSensorSourceExists()
        {
            // 默认侦测范围
            // detectionLayer = LayerMask.GetMask("Player");
            if (sensorSource) return;
            sensorSource = new GameObject("SensorSource");
            sensorSource.transform.SetParent(transform);
            sensorSource.transform.localPosition = Vector3.zero; // 调整高度
        }

        private void Scan()
        {

            detectedTargets.Clear();

            var hits = Physics.OverlapSphere(sensorSource.transform.position, viewDistance, detectionLayer);

            foreach (var hit in hits)
            {
                var dirToTarget = (hit.transform.position - sensorSource.transform.position).normalized;
                var angleToTarget = Vector3.Angle(sensorSource.transform.forward, dirToTarget);

                if (angleToTarget < viewAngle / 2f)
                {
                    detectedTargets.Add(hit.transform);
                }
            }
        }
        //
        // // 可视化锥形视野
        // private void OnDrawGizmos()
        // {
        //     if (sensorSource == null) return;
        //
        //     var origin = sensorSource.transform.position;
        //     var forward = sensorSource.transform.forward;
        //
        //     Gizmos.color = Color.green;
        //     Gizmos.DrawWireSphere(origin, viewDistance);
        //
        //     // 画扇形边缘
        //     Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * forward;
        //     Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * forward;
        //
        //     Gizmos.color = Color.yellow;
        //     Gizmos.DrawLine(origin, origin + left * viewDistance);
        //     Gizmos.DrawLine(origin, origin + right * viewDistance);
        //
        //     // 画锥体填充区域（可选）
        //     Gizmos.color = new Color(1, 1, 0, 0.1f);
        //     Gizmos.DrawRay(origin, left * viewDistance);
        //     Gizmos.DrawRay(origin, right * viewDistance);
        //
        //     // 画出射线投射
        //     foreach (var target in detectedTargets)
        //     {
        //         Gizmos.color = Color.red;
        //         Gizmos.DrawLine(sensorSource.transform.position, target.position);
        //     }
        // }



    }
}
