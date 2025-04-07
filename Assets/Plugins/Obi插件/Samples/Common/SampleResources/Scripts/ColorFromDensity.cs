using UnityEngine;

namespace Obi.Samples
{
	[RequireComponent(typeof(ObiEmitter))]
	public class ColorFromDensity : MonoBehaviour
	{
		ObiEmitter emitter;
		public Gradient grad;

		void Awake()
        {
			emitter = GetComponent<ObiEmitter>();

            emitter.OnBlueprintLoaded += Emitter_OnBlueprintLoaded;
            emitter.OnBlueprintUnloaded += Emitter_OnBlueprintUnloaded;

            if (emitter.isLoaded)
                Emitter_OnBlueprintLoaded(emitter, emitter.sourceBlueprint);
        }

        private void OnDestroy()
        {
            emitter.OnBlueprintLoaded -= Emitter_OnBlueprintLoaded;
            emitter.OnBlueprintUnloaded -= Emitter_OnBlueprintUnloaded;
        }

        private void Emitter_OnBlueprintLoaded(ObiActor actor, ObiActorBlueprint blueprint)
        {
            actor.solver.OnRequestReadback += Solver_OnRequestReadback;
            actor.solver.OnSimulationEnd += Solver_OnSimulationEnd;
        }

        private void Emitter_OnBlueprintUnloaded(ObiActor actor, ObiActorBlueprint blueprint)
        {
            actor.solver.OnRequestReadback -= Solver_OnRequestReadback;
            actor.solver.OnSimulationEnd -= Solver_OnSimulationEnd;
        }

        private void Solver_OnRequestReadback(ObiSolver solver)
        {
            solver.fluidData.Readback();
        }

        private void Solver_OnSimulationEnd(ObiSolver solver, float timeToSimulate, float substepTime)
        {
            solver.fluidData.WaitForReadback();

            int dimensions = 3 - (int)solver.parameters.mode; // 2 in 2D mode, 3 in 3D mode.

            for (int i = 0; i < emitter.solverIndices.count; ++i)
            {
                int k = emitter.solverIndices[i];

                var density = emitter.solver.fluidData[k].x;
                var volume = Mathf.Pow(emitter.solver.principalRadii[k].x * 2, dimensions);

                emitter.solver.colors[k] = grad.Evaluate(density * volume - 1);
            }
        }
	
	}
}

