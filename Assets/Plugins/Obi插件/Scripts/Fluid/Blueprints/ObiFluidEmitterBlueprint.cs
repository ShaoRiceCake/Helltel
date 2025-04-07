using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Obi
{

    [CreateAssetMenu(fileName = "fluid blueprint", menuName = "Obi/Fluid Blueprint", order = 100)]
    public class ObiFluidEmitterBlueprint : ObiEmitterBlueprintBase
    {
        // fluid parameters:
        [Min(1)]
        public float smoothing = 2f;

        public float viscosity = 0.05f;         /**< viscosity of the fluid*/

        public float polarity = 1f;       /**< higher polarity leads to higher surface tension. fluid of similar polarity are miscible.*/

        public float vorticity = 0.0f;                      /**< amount of vorticity confinement.*/

        // gas parameters:
        public float buoyancy = -1.0f;                      /**< how dense is this material with respect to air?*/

        [Min(0)]
        public float atmosphericDrag = 0.0f;                /**< amount of drag applied by the surrounding air to particles near the surface of the material.*/
        public float atmosphericPressure = 0.0f;            /**< amount of pressure applied by the surrounding air particles.*/

        public float miscibility = 0.0f;
        public Vector4 userData;                       /**< values affected by miscibility*/

        public void OnValidate()
        {
            viscosity = Mathf.Clamp01(viscosity);
        }

        public float GetSmoothingRadius(Oni.SolverParameters.Mode mode)
        {
            return GetParticleSize(mode) * smoothing;
        }
    }
}