using System;
using UnityEngine;
using Obi;

public class ExternalCollisionSolidify : MonoBehaviour
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

    private ObiSolver _solver;
    public Color solidColor;

    private SolidData[] _solids = Array.Empty<SolidData>();

    public void Awake()
    {
        _solver = GetComponent<ObiSolver>();
    }

    private void OnEnable()
    {
        _solver.OnSimulationStart += Solver_OnBeginStep;
        _solver.OnCollision += Solver_OnCollision;
    }

    private void OnDisable()
    {
        _solver.OnSimulationStart -= Solver_OnBeginStep;
        _solver.OnCollision -= Solver_OnCollision;
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

    private void Solver_OnBeginStep(ObiSolver s, float timeToSimulate, float substepTime)
    {
        if (_solver.allocParticleCount > _solids.Length)
        {
            Array.Resize(ref _solids, _solver.allocParticleCount);
        }

        // Update positions of already solidified particles
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
            particleIndex >= _solids.Length || !solid.Reference)
            return;

        _solver.phases[particleIndex] &= (int)(~ObiUtils.ParticleFlags.Fluid);
        _solver.invMasses[particleIndex] = 0;
        _solver.colors[particleIndex] = solidColor;

        solid.LocalPos = solid.Reference.InverseTransformPoint(
            _solver.transform.TransformPoint(_solver.positions[particleIndex]));
        _solids[particleIndex] = solid;
    }
}