﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Obi.Samples
{
    [RequireComponent(typeof(ObiActor))]
    public class AddRandomVelocity : MonoBehaviour
    {

        public float intensity = 5;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GetComponent<ObiActor>().AddForce(UnityEngine.Random.onUnitSphere * intensity, ForceMode.VelocityChange);
            }
        }
    }
}
