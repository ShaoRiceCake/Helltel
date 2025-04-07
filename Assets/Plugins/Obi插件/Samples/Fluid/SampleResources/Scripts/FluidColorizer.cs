using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Obi.Samples
{
    [RequireComponent(typeof(ObiCollider))]
    public class FluidColorizer : MonoBehaviour
    {
        public Color color;
        public float tintSpeed = 5;
        public ObiCollider obiCollider;

        void Awake()
        {
            obiCollider = GetComponent<ObiCollider>();
        }
    }
}
