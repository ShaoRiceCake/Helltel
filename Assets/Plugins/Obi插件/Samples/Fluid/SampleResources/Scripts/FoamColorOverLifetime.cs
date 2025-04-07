using UnityEngine;

namespace Obi.Samples
{
    [RequireComponent(typeof(ObiSolver))]
    public class FoamColorOverLifetime : MonoBehaviour
    {
        public Gradient gradient;

        void OnEnable()
        {
            GetComponent<ObiSolver>().OnAdvection += FoamColorOverLifetime_OnAdvection;
        }

        void OnDisable()
        {
            GetComponent<ObiSolver>().OnAdvection -= FoamColorOverLifetime_OnAdvection;
        }

        private void FoamColorOverLifetime_OnAdvection(ObiSolver solver)
        {
            for (int i = 0; i < solver.foamCount[3]; ++i)
                solver.foamColors[i] = gradient.Evaluate(1 - solver.foamAttributes[i].x);
        }
    }
}
