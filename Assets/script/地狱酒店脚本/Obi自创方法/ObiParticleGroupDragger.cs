using UnityEngine;

namespace Obi
{
    public class ObiParticleGroupDragger : ObiParticleGroupTool
    {
        public float springStiffness = 500; // ���ɸն�
        public float springDamping = 50;    // ��������
        public Transform targetPoint;      // Ŀ���
        public bool debugMode = false;

        private bool isDragging = false;

        public bool IsDragging
        {
            get => isDragging;
            set => isDragging = value;
        }

        public Transform TargetPoint
        {
            get => targetPoint;
            set => targetPoint = value;
        }

        void DebugMode()
        {
            if (debugMode)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    isDragging = true;
                }
                if (Input.GetMouseButtonUp(0))
                {
                    isDragging = false;
                }
            }
        }

        private void Update()
        {
            DebugMode();
        }

        void FixedUpdate()
        {
            if (isDragging && solver != null && targetPoint != null && particleGroupIndices != null)
            {
                Vector4 targetPosition = solver.transform.InverseTransformPoint(targetPoint.position);

                foreach (int particleIndex in particleGroupIndices)
                {
                    if (particleIndex >= 0 && particleIndex < solver.positions.count)
                    {
                        float invMass = solver.invMasses[particleIndex];
                        if (invMass > 0)
                        {
                            Vector4 position = solver.positions[particleIndex];
                            Vector4 velocity = solver.velocities[particleIndex];
                            solver.externalForces[particleIndex] = ((targetPosition - position) * springStiffness - velocity * springDamping) / invMass;
                        }
                    }
                }
            }
        }
    }
}