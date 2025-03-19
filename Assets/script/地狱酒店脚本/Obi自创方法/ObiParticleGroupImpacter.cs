using UnityEngine;
using System.Collections;

namespace Obi
{
    public class ObiParticleGroupImpacter : ObiParticleGroupTool
    {
        public float impulseMagnitude = 50f; // �������С
        public Vector3 impulseDirection = Vector3.up; // ���������
        public float impulseDuration = 0.1f; // ���������ʱ��

        private bool isImpacting = false; // �Ƿ�����ʩ�ӳ����

        // ʩ�ӳ������Э��
        private IEnumerator ApplyImpulse()
        {
            isImpacting = true;

            // ���������ķ��򣨹�һ����
            Vector3 normalizedDirection = impulseDirection.normalized;

            // ���������飬��ÿ������ʩ�ӳ����
            foreach (int particleIndex in particleGroupIndices)
            {
                if (particleIndex >= 0 && particleIndex < solver.positions.count)
                {
                    float invMass = solver.invMasses[particleIndex];
                    if (invMass > 0)
                    {
                        // ��������
                        Vector4 impulse = (Vector4)(normalizedDirection * impulseMagnitude / invMass);

                        // ʩ�ӳ����
                        solver.velocities[particleIndex] += impulse;
                    }
                }
            }

            // �ȴ����������ʱ��
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