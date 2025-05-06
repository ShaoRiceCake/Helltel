using UnityEngine;
using Obi;

[ExecuteInEditMode]
public class CustomObiRigidbody : ObiRigidbody
{
    public override void UpdateVelocities(Vector3 linearDelta, Vector3 angularDelta)
    {
        if (!unityRigidbody) return;

        if (!Application.isPlaying || (unityRigidbody.isKinematic || kinematicForParticles)) return;
        unityRigidbody.velocity += linearDelta;

        if (!unityRigidbody.freezeRotation)
        {
            unityRigidbody.angularVelocity += angularDelta;
        }
    }
}