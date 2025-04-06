using System;
using UnityEngine;
using Obi;

public class GradualSolidifyOnContact : MonoBehaviour
{
    private struct SolidData
    {
        public readonly Transform Reference;
        public Vector3 LocalPos;

        public SolidData(Transform reference)
        {
            this.Reference = reference;
            this.LocalPos = Vector3.zero;
        }
    };

    private struct DelayedSolidification
    {
        public int ParticleIndex;
        public int FramesRemaining;
        public SolidData SolidData;
    }

    private ObiSolver _solver;
    public Color solidColor;

    [Tooltip("Number of frames to delay solidification when particles collide with already solidified particles")]
    public int delayFrames;

    private SolidData[] _solids = Array.Empty<SolidData>();

    private readonly System.Collections.Generic.List<DelayedSolidification> _delayedParticles =
        new System.Collections.Generic.List<DelayedSolidification>();

    public void Awake()
    {
        _solver = GetComponent<ObiSolver>();
    }

    private void OnEnable()
    {
        _solver.OnSimulationStart += Solver_OnBeginStep;
        _solver.OnCollision += Solver_OnCollision;
        _solver.OnParticleCollision += Solver_OnParticleCollision;
    }

    private void OnDisable()
    {
        _solver.OnSimulationStart -= Solver_OnBeginStep;
        _solver.OnCollision -= Solver_OnCollision;
        _solver.OnParticleCollision -= Solver_OnParticleCollision;
    }

    private void Solver_OnCollision(object sender, ObiNativeContactList e)
    {
        // Ensure solids array is large enough
        if (_solver.allocParticleCount > _solids.Length)
        {
            Array.Resize(ref _solids, _solver.allocParticleCount);
        }

        var colliderWorld = ObiColliderWorld.GetInstance();
        if (colliderWorld == null) return;

        for (var i = 0; i < e.count; ++i)
        {
            if (!(e[i].distance < 0.001f)) continue;
            var handle = colliderWorld.colliderHandles[e[i].bodyB];
            if (handle == null || !handle.owner) continue;

            var col = handle.owner;
            if (!col || !col.transform) continue;
            var particleIndex = _solver.simplices[e[i].bodyA];
            if (particleIndex >= 0 && particleIndex < _solids.Length)
            {
                Solidify(particleIndex, new SolidData(col.transform));
            }
        }
    }

    private void Solver_OnParticleCollision(object sender, ObiNativeContactList e)
    {
        if (_solver.allocParticleCount > _solids.Length)
        {
            Array.Resize(ref _solids, _solver.allocParticleCount);
        }

        for (var i = 0; i < e.count; ++i)
        {
            if (!(e[i].distance < 0.001f)) continue;
            var particleIndexA = _solver.simplices[e[i].bodyA];
            var particleIndexB = _solver.simplices[e[i].bodyB];

            if (particleIndexA < 0 || particleIndexA >= _solver.invMasses.count || 
                particleIndexB < 0 || particleIndexB >= _solver.invMasses.count)
                continue;

            if (_solver.invMasses[particleIndexA] < 0.0001f && _solver.invMasses[particleIndexB] >= 0.0001f)
            {
                if (particleIndexA < _solids.Length)
                {
                    if (delayFrames > 0)
                    {
                        _delayedParticles.Add(new DelayedSolidification()
                        {
                            ParticleIndex = particleIndexB,
                            FramesRemaining = delayFrames,
                            SolidData = _solids[particleIndexA]
                        });
                    }
                    else
                    {
                        Solidify(particleIndexB, _solids[particleIndexA]);
                    }
                }
            }

            if (!(_solver.invMasses[particleIndexB] < 0.0001f) ||
                !(_solver.invMasses[particleIndexA] >= 0.0001f)) continue;
            if (particleIndexB >= _solids.Length) continue;
            if (delayFrames > 0)
            {
                _delayedParticles.Add(new DelayedSolidification()
                {
                    ParticleIndex = particleIndexA,
                    FramesRemaining = delayFrames,
                    SolidData = _solids[particleIndexB]
                });
            }
            else
            {
                Solidify(particleIndexA, _solids[particleIndexB]);
            }
        }
    }

    private void Solver_OnBeginStep(ObiSolver s, float timeToSimulate, float substepTime)
    {
        if (_solver.allocParticleCount > _solids.Length)
        {
            Array.Resize(ref _solids, _solver.allocParticleCount);
        }

        for (var i = _delayedParticles.Count - 1; i >= 0; i--)
        {
            var delayed = _delayedParticles[i];
            delayed.FramesRemaining--;

            if (delayed.FramesRemaining <= 0)
            {
                if (delayed.ParticleIndex >= 0 && delayed.ParticleIndex < _solver.invMasses.count && 
                    delayed.ParticleIndex < _solids.Length)
                {
                    Solidify(delayed.ParticleIndex, delayed.SolidData);
                }
                _delayedParticles.RemoveAt(i);
            }
            else
            {
                _delayedParticles[i] = delayed;
            }
        }

        for (var i = 0; i < _solids.Length; ++i)
        {
            if (i < _solver.invMasses.count && _solver.invMasses[i] < 0.0001f && 
                _solids[i].Reference)
            {
                _solver.positions[i] = _solver.transform.InverseTransformPoint(
                    _solids[i].Reference.TransformPoint(_solids[i].LocalPos));
            }
        }
    }

    private void Solidify(int particleIndex, SolidData solid)
    {
        if (particleIndex < 0 || particleIndex >= _solver.phases.count || 
            particleIndex >= _solids.Length || solid.Reference == null)
            return;

        _solver.phases[particleIndex] &= (int)(~ObiUtils.ParticleFlags.Fluid);
        _solver.invMasses[particleIndex] = 0;
        _solver.colors[particleIndex] = solidColor;

        solid.LocalPos = solid.Reference.InverseTransformPoint(
            _solver.transform.TransformPoint(_solver.positions[particleIndex]));
        _solids[particleIndex] = solid;
    }
}