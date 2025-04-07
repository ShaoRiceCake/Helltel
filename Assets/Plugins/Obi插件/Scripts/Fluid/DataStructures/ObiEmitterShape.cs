using UnityEngine;
using System.Collections.Generic;

namespace Obi
{

	[ExecuteInEditMode]
	public abstract class ObiEmitterShape : MonoBehaviour
	{
		[SerializeProperty("Emitter")]
		[SerializeField] protected ObiEmitter emitter;

		public Color color = Color.white;

		[HideInInspector] public float particleSize = 0;
		[HideInInspector] public List<EmitPoint> distribution = new List<EmitPoint>();

        protected Matrix4x4 prevl2sTransform;
        protected Matrix4x4 l2sTransform;
        public float deltaTime { get; private set; }

        public ObiEmitter Emitter{
			set{
				if (emitter != value){

					if (emitter != null){
						emitter.RemoveShape(this);
					}

					emitter = value;
					
					if (emitter != null){
						emitter.AddShape(this);
					}
				}
			}
			get{return emitter;}
		}

		public Matrix4x4 ShapeLocalToSolverMatrix
        {
			get{return l2sTransform;}
		}

        public Matrix4x4 PreviousShapeLocalToSolverMatrix
        {
            get { return prevl2sTransform; }
        }

        public void OnEnable()
        {
			if (emitter != null)
				emitter.AddShape(this);
		}

		public void OnDisable()
        {
			if (emitter != null)
				emitter.RemoveShape(this);
		}

		public void UpdateLocalToSolverMatrix(float deltaTime)
        {
            this.deltaTime = deltaTime;

            prevl2sTransform = l2sTransform;
            if (emitter != null && emitter.solver != null){
				l2sTransform = emitter.solver.transform.worldToLocalMatrix * transform.localToWorldMatrix;
			}else{
				l2sTransform = transform.localToWorldMatrix;
			}
		}

		public abstract void GenerateDistribution();
		
	}
}

