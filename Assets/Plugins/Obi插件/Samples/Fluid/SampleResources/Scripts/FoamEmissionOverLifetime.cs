using UnityEngine;
using System.Collections.Generic;

namespace Obi.Samples
{
    [RequireComponent(typeof(ObiEmitter))]
    public class FoamEmissionOverLifetime : MonoBehaviour
    {
        ObiEmitter emitter;
        List<int> emitPositions = new List<int>();

        public int emissionRate = 5;
        public float particleSize = 0.01f;
        public float particleLifetime = 4f;

        void Awake()
        {
            emitter = GetComponent<ObiEmitter>();
            emitter.OnBlueprintLoaded += FoamEmissionOverLifetime_OnBlueprintLoaded;
            emitter.OnBlueprintUnloaded += Emitter_OnBlueprintUnloaded;

            if (emitter.isLoaded)
                FoamEmissionOverLifetime_OnBlueprintLoaded(emitter, emitter.sharedBlueprint);

            emitter.OnEmitParticle += Emitter_OnEmitParticle;
        }

        void OnDestroy()
        {
            emitter.OnBlueprintLoaded -= FoamEmissionOverLifetime_OnBlueprintLoaded;
            emitter.OnBlueprintUnloaded -= Emitter_OnBlueprintUnloaded;
            emitter.OnEmitParticle -= Emitter_OnEmitParticle;
        }

        private void FoamEmissionOverLifetime_OnBlueprintLoaded(ObiActor actor, ObiActorBlueprint blueprint)
        {
            actor.solver.OnAdvection += FoamColorOverLifetime_OnAdvection;
        }

        private void Emitter_OnBlueprintUnloaded(ObiActor actor, ObiActorBlueprint blueprint)
        {
            actor.solver.OnAdvection -= FoamColorOverLifetime_OnAdvection;
        }

        private void Emitter_OnEmitParticle(ObiEmitter emt, int particleIndex)
        {
            emitPositions.Add(particleIndex);
        }

        private void FoamColorOverLifetime_OnAdvection(ObiSolver solver)
        {
            for (int i = 0; i < emitPositions.Count; ++i)
            {
                for (int j = 0; j < emissionRate; ++j)
                {
                    if (solver.foamCount[3] < solver.maxFoamParticles)
                    {
                        int p = solver.foamCount[3]++;
                        int fluidIndex = emitter.solverIndices[emitPositions[i]];

                        solver.foamPositions[p] = solver.positions[fluidIndex] + (Vector4)Random.insideUnitSphere * solver.principalRadii[fluidIndex].x * 1.2f;
                        solver.foamVelocities[p] = Vector4.zero;
                        solver.foamAttributes[p] = new Vector4(1, 1 / particleLifetime, particleSize, ObiUtils.PackFloatRGBA(new Vector4(0, 0, 1, 0)));
                        solver.foamColors[p] = Color.red;
                    }
                }
            }

            emitPositions.Clear();
        }
    }
}