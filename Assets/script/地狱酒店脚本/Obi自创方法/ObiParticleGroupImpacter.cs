using UnityEngine;
using System.Collections;

namespace Obi
{
    public class ObiParticleGroupImpacter : ObiParticleGroupTool
    {
        public float impulseMagnitude = 50f; // 冲击力大小
        public Vector3 impulseDirection = Vector3.up; // 冲击力方向
        public float impulseDuration = 0.1f; // 冲击力作用时间

        private bool isImpacting = false; // 是否正在施加冲击力

        // 施加冲击力的协程
        private IEnumerator ApplyImpulse()
        {
            isImpacting = true;

            // 计算冲击力的方向（归一化）
            Vector3 normalizedDirection = impulseDirection.normalized;

            // 遍历粒子组，对每个粒子施加冲击力
            foreach (int particleIndex in particleGroupIndices)
            {
                if (particleIndex >= 0 && particleIndex < solver.positions.count)
                {
                    float invMass = solver.invMasses[particleIndex];
                    if (invMass > 0)
                    {
                        // 计算冲击力
                        Vector4 impulse = (Vector4)(normalizedDirection * impulseMagnitude / invMass);

                        // 施加冲击力
                        solver.velocities[particleIndex] += impulse;
                    }
                }
            }

            // 等待冲击力作用时间
            yield return new WaitForSeconds(impulseDuration);

            isImpacting = false;
        }

        public void TriggerImpulse(Vector3 direction, float magnitude, float duration)
        {
            if (!isImpacting)
            {
                impulseDirection = direction;
                impulseMagnitude = magnitude;
                impulseDuration = duration;
                StartCoroutine(ApplyImpulse());
            }
        }
    }
}