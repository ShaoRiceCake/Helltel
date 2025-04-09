using UnityEngine;
using Obi;
using System.Collections.Generic;

public class MultiPositionEmitter : MonoBehaviour
{
    public CustomEmitterController emitterController;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            emitterController.Emit();
        }
    }
}