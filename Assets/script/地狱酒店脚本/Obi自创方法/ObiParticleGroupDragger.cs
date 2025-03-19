using UnityEngine;

namespace Obi
{
    public class ObiParticleGroupDragger : ObiParticleGroupTool
    {
        public float springStiffness = 500; // µ¯»É¸Õ¶È
        public float springDamping = 50;    // µ¯»É×èÄá
        public Transform targetPoint;      // Ä¿±êµã

        private bool isDragging = false;

        public bool IsDragging
        {
            get => isDragging;
            set => isDragging = value;
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