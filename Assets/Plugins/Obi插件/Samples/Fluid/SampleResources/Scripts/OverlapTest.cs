using UnityEngine;

namespace Obi.Samples
{
    public class OverlapTest : MonoBehaviour
    {
        public ObiSolver solver;
        public Transform[] cubes;

        private void Start()
        {
            solver.OnSpatialQueryResults += Solver_OnSpatialQueryResults;
            solver.OnSimulationStart += Solver_OnSimulate;
        }

        private void OnDestroy()
        {
            solver.OnSpatialQueryResults -= Solver_OnSpatialQueryResults;
            solver.OnSimulationStart -= Solver_OnSimulate;
        }

        private void Solver_OnSpatialQueryResults(ObiSolver s, ObiNativeQueryResultList queryResults)
        {
            for (int i = 0; i < solver.colors.count; ++i)
                solver.colors[i] = Color.cyan;

            // Iterate over results and draw their distance to the center of the cube.
            // We're assuming the solver only contains 0-simplices (particles).
            for (int i = 0; i < queryResults.count; ++i)
            {
                if (queryResults[i].distance < 0)
                {
                    int particleIndex = solver.simplices[queryResults[i].simplexIndex];

                    if (queryResults[i].queryIndex == 0)
                        solver.colors[particleIndex] = Color.red;
                    else if (queryResults[i].queryIndex == 1)
                        solver.colors[particleIndex] = Color.yellow;
                }
            }

            solver.colors.Upload();
        }

        private void Solver_OnSimulate(ObiSolver s, float timeToSimulate, float substepTime)
        {
            int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);

            for (int i = 0; i < cubes.Length; ++i)
            {
                solver.EnqueueSpatialQuery(new QueryShape
                {
                    type = QueryShape.QueryType.Box,
                    center = Vector3.zero,
                    size = new Vector3(1, 1, 1),
                    contactOffset = 0,
                    maxDistance = 0,
                    filter = filter
                }, new AffineTransform(cubes[i].position, cubes[i].rotation, cubes[i].localScale));
            }
        }
    }
}