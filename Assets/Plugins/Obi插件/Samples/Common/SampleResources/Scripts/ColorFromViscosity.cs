using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi.Samples
{
	/**
	 * Sample script that colors fluid particles based on their vorticity (2D only)
	 */
	[RequireComponent(typeof(ObiEmitter))]
	public class ColorFromViscosity : MonoBehaviour
	{
		ObiEmitter emitter;

		public float min = 0;
		public float max = 1;
		public Gradient grad;

		void Awake()
        {
			emitter = GetComponent<ObiEmitter>();
		}

		void LateUpdate()
		{
			if (!isActiveAndEnabled || !emitter.isLoaded)
				return;

			for (int i = 0; i < emitter.solverIndices.count; ++i)
            {
				int k = emitter.solverIndices[i];

                var param = emitter.solver.fluidMaterials[k];
                emitter.solver.colors[k] = grad.Evaluate((param.z - min) / (max - min));
                param.z = emitter.solver.userData[k][0];
                param.y = emitter.solver.userData[k][1];
                emitter.solver.fluidMaterials[k] = param;
			}
		}
	
	}
}

