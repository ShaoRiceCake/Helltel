using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Obi
{
    public abstract class ObiParticleGroupTool : MonoBehaviour
    {
        public ObiParticleAttachment obiParticleAttachment; // 粒子组附件
        protected ObiSolver solver; // Obi物理求解器
        protected List<int> particleGroupIndices; // 粒子组的索引

        protected virtual IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            particleGroupIndices = ObiGetGroupParticles.GetParticleSolverIndices(obiParticleAttachment);

            if (!obiParticleAttachment)
            {
                Debug.LogError("No obiParticleAttachment found. -- " + GetType().Name);
            }
            else
            {
                solver = obiParticleAttachment.actor.solver;
                if (solver == null)
                {
                    Debug.LogError("No solver found. -- " + GetType().Name);
                }
            }

            if (particleGroupIndices == null || particleGroupIndices.Count == 0)
            {
                Debug.LogError("No particle indices found. -- " + GetType().Name);
            }
        }
    }
}