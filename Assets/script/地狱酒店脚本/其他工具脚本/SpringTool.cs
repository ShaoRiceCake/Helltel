using UnityEngine;

public class SpringTool : MonoBehaviour
{
    public Transform target;

    public GameObject controlledObject;

    public float springForce = 5f;

    public float damping = 0.5f;

    public bool isSpringEnabled ;

    private Vector3 _velocity;

    private Rigidbody _movingRb;

    private void FixedUpdate()
    {
        if (isSpringEnabled && target && controlledObject != null)
        {
            ApplySpring();
        }
    }

    private void ApplySpring()
    {
        var displacement = target.position - controlledObject.transform.position;
        var distance = displacement.magnitude;

        var springForceVector = displacement.normalized * (distance * springForce);

        var dampingForce = -_movingRb.velocity * damping;

        _movingRb.AddForce(springForceVector + dampingForce);
    }
    
    public void EnableSpring()
    {
        isSpringEnabled = true;
    }
    
    public void DisableSpring()
    {
        isSpringEnabled = false;
    }
    
    public void SetSpringForce(float force)
    {
        springForce = force;
    }
    
    public void SetDamping(float dampingValue)
    {
        damping = dampingValue;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void SetControlledObject(GameObject newControlledObject)
    {
        controlledObject = newControlledObject;
        _movingRb = controlledObject.GetComponent<Rigidbody>();
        if (!_movingRb)
        {
            Debug.LogError("movingRb is null!");
        }
    }
}