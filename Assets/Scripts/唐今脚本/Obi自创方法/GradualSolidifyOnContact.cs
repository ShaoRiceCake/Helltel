using System;
using UnityEngine;
using Obi;

public class GradualSolidifyOnContact : MonoBehaviour
{
    public struct SolidData
    {
        public Transform reference;
        public Vector3 localPos;

        public SolidData(Transform reference)
        {
            this.reference = reference;
            this.localPos = Vector3.zero;
        }
    };

    public struct DelayedSolidification
    {
        public int particleIndex;
        public int framesRemaining;
        public SolidData solidData;
    }

    ObiSolver solver;
    public Color solidColor;

    [Tooltip("Number of frames to delay solidification when particles collide with already solidified particles")]
    public int delayFrames = 0;

    public SolidData[] solids = new SolidData[0];

    private System.Collections.Generic.List<DelayedSolidification> delayedParticles =
        new System.Collections.Generic.List<DelayedSolidification>();

    void Awake()
    {
        solver = GetComponent<ObiSolver>();
    }

    void OnEnable()
    {
        solver.OnSimulationStart += Solver_OnBeginStep;
        solver.OnCollision += Solver_OnCollision;
        solver.OnParticleCollision += Solver_OnParticleCollision;
    }

    void OnDisable()
    {
        solver.OnSimulationStart -= Solver_OnBeginStep;
        solver.OnCollision -= Solver_OnCollision;
        solver.OnParticleCollision -= Solver_OnParticleCollision;
    }

    void Solver_OnCollision(object sender, ObiNativeContactList e)
    {
        // Ensure solids array is large enough
        if (solver.allocParticleCount > solids.Length)
        {
            Array.Resize(ref solids, solver.allocParticleCount);
        }

        var colliderWorld = ObiColliderWorld.GetInstance();
        if (colliderWorld == null) return;

        for (int i = 0; i < e.count; ++i)
        {
            if (e[i].distance < 0.001f)
            {
                var handle = colliderWorld.colliderHandles[e[i].bodyB];
                if (handle == null || handle.owner == null) continue;

                var col = handle.owner;
                if (col != null && col.transform != null)
                {
                    int particleIndex = solver.simplices[e[i].bodyA];
                    if (particleIndex >= 0 && particleIndex < solids.Length)
                    {
                        Solidify(particleIndex, new SolidData(col.transform));
                    }
                }
            }
        }
    }

    void Solver_OnParticleCollision(object sender, ObiNativeContactList e)
    {
        // Ensure solids array is large enough
        if (solver.allocParticleCount > solids.Length)
        {
            Array.Resize(ref solids, solver.allocParticleCount);
        }

        for (int i = 0; i < e.count; ++i)
        {
            if (e[i].distance < 0.001f)
            {
                int particleIndexA = solver.simplices[e[i].bodyA];
                int particleIndexB = solver.simplices[e[i].bodyB];

                if (particleIndexA < 0 || particleIndexA >= solver.invMasses.count || 
                    particleIndexB < 0 || particleIndexB >= solver.invMasses.count)
                    continue;

                if (solver.invMasses[particleIndexA] < 0.0001f && solver.invMasses[particleIndexB] >= 0.0001f)
                {
                    if (particleIndexA >= 0 && particleIndexA < solids.Length)
                    {
                        if (delayFrames > 0)
                        {
                            delayedParticles.Add(new DelayedSolidification()
                            {
                                particleIndex = particleIndexB,
                                framesRemaining = delayFrames,
                                solidData = solids[particleIndexA]
                            });
                        }
                        else
                        {
                            Solidify(particleIndexB, solids[particleIndexA]);
                        }
                    }
                }

                if (solver.invMasses[particleIndexB] < 0.0001f && solver.invMasses[particleIndexA] >= 0.0001f)
                {
                    if (particleIndexB >= 0 && particleIndexB < solids.Length)
                    {
                        if (delayFrames > 0)
                        {
                            delayedParticles.Add(new DelayedSolidification()
                            {
                                particleIndex = particleIndexA,
                                framesRemaining = delayFrames,
                                solidData = solids[particleIndexB]
                            });
                        }
                        else
                        {
                            Solidify(particleIndexA, solids[particleIndexB]);
                        }
                    }
                }
            }
        }
    }

    void Solver_OnBeginStep(ObiSolver s, float timeToSimulate, float substepTime)
    {
        // Ensure solids array is large enough
        if (solver.allocParticleCount > solids.Length)
        {
            Array.Resize(ref solids, solver.allocParticleCount);
        }

        // Process delayed solidifications
        for (int i = delayedParticles.Count - 1; i >= 0; i--)
        {
            var delayed = delayedParticles[i];
            delayed.framesRemaining--;

            if (delayed.framesRemaining <= 0)
            {
                if (delayed.particleIndex >= 0 && delayed.particleIndex < solver.invMasses.count && 
                    delayed.particleIndex < solids.Length)
                {
                    Solidify(delayed.particleIndex, delayed.solidData);
                }
                delayedParticles.RemoveAt(i);
            }
            else
            {
                delayedParticles[i] = delayed;
            }
        }

        // Original behavior for already solidified particles
        for (int i = 0; i < solids.Length; ++i)
        {
            if (i < solver.invMasses.count && solver.invMasses[i] < 0.0001f && 
                solids[i].reference != null)
            {
                solver.positions[i] = solver.transform.InverseTransformPoint(
                    solids[i].reference.TransformPoint(solids[i].localPos));
            }
        }
    }

    void Solidify(int particleIndex, SolidData solid)
    {
        if (particleIndex < 0 || particleIndex >= solver.phases.count || 
            particleIndex >= solids.Length || solid.reference == null)
            return;

        // remove the 'fluid' flag from the particle, turning it into a solid granule:
        solver.phases[particleIndex] &= (int)(~ObiUtils.ParticleFlags.Fluid);

        // fix the particle in place (by giving it infinite mass):
        solver.invMasses[particleIndex] = 0;

        // and change its color:
        solver.colors[particleIndex] = solidColor;

        // set the solid data for this particle:
        solid.localPos = solid.reference.InverseTransformPoint(
            solver.transform.TransformPoint(solver.positions[particleIndex]));
        solids[particleIndex] = solid;
    }
}