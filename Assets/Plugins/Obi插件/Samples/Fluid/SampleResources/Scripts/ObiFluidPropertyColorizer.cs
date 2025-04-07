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
	public class ObiFluidPropertyColorizer : MonoBehaviour
	{
		ObiEmitter emitter;

		public Gradient grad;

		void Awake(){
			emitter = GetComponent<ObiEmitter>();
		}

		public void OnEnable(){}

		void LateUpdate()
		{
			if (!isActiveAndEnabled || !emitter.isLoaded)
				return;

			for (int i = 0; i < emitter.solverIndices.count; ++i)
            {
                int k = emitter.solverIndices[i];
                emitter.solver.colors[k] = grad.Evaluate(emitter.solver.userData[k][0]);
			}
		}
	
	}
}

