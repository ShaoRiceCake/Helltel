using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
    [RequireComponent(typeof(ObiParticlePicker))]
    public class ObiParticleGroupDragger : MonoBehaviour
    {
        public ObiSolver solver;
        public float springStiffness = 500; // 弹簧刚度
        public float springDamping = 50;    // 弹簧阻尼
        public Transform targetPoint;      // 目标点
        public ObiParticleAttachment obiParticleAttachment;

        private List<int> particleGroupIndices; // 粒子组的索引
        private bool isDragging = false;

        private void Start()
        {
            particleGroupIndices = ObiGetGroupParticles.GetParticleSolverIndices(obiParticleAttachment);
            if (particleGroupIndices == null || particleGroupIndices.Count == 0)
            {
                Debug.LogError("No particle indices found. Check obiParticleAttachment and GetParticleSolverIndices method.");
            }
            else
            {
                Debug.Log("Particle indices: " + string.Join(", ", particleGroupIndices));
            }
        }

        void Update()
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