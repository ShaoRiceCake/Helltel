using UnityEngine;
using Obi;

public class ControlBallGenerator : MonoBehaviour
{
    public static GameObject GenerateControlBall()
    {
        var controlBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        controlBall.name = "ControlBall";
        controlBall.tag = "Uncatchable";
        
        controlBall.transform.localScale = Vector3.one * 0.2f;

        var sphereCollider = controlBall.GetComponent<SphereCollider>();
        if (!sphereCollider)
        {
            controlBall.AddComponent<SphereCollider>();
        }
        
        var renderer = controlBall.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        var obiCollider = controlBall.AddComponent<ObiCollider>();
        obiCollider.Filter = ObiUtils.MakeFilter(ObiUtils.CollideWithNothing, 0);

        var rigidbody = controlBall.AddComponent<Rigidbody>();
        rigidbody.mass = 1f;
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        var obiRigidbody = controlBall.AddComponent<ObiRigidbody>();
        obiRigidbody.kinematicForParticles = true;

        return controlBall;
    }
}