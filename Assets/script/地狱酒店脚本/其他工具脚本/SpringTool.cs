using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class SpringTool : MonoBehaviour
{
    public Transform target;

    public GameObject controlledObject;

    public float springForce = 5f;

    public float damping = 0.5f;

    public bool isSpringEnabled = false;

    private Vector3 _velocity;

    private Rigidbody movingRb;

    void FixedUpdate()
    {
        if (isSpringEnabled && target != null && controlledObject != null)
        {
            ApplySpring();
        }
    }

    private void ApplySpring()
    {
        // ¼ÆËãÄ¿±ê¶ÔÏóºÍ²Ù¿Ø¶ÔÏóÖ®¼äµÄÎ»ÒÆ
        Vector3 displacement = target.position - controlledObject.transform.position;
        float distance = displacement.magnitude;

        // ¼ÆËãµ¯»ÉÁ¦
        Vector3 springForceVector = displacement.normalized * distance * springForce;

        // ¼ÆËã×èÄáÁ¦
        Vector3 dampingForce = -movingRb.velocity * damping;

        // Ê©¼ÓºÏÁ¦
        movingRb.AddForce(springForceVector + dampingForce);
    }

    // ¹«¿ª·½·¨£ºÆôÓÃµ¯»ÉÐ§¹û
    public void EnableSpring()
    {
        isSpringEnabled = true;
    }

    // ¹«¿ª·½·¨£º½ûÓÃµ¯»ÉÐ§¹û
    public void DisableSpring()
    {
        isSpringEnabled = false;
    }

    // ¹«¿ª·½·¨£ºÉèÖÃµ¯»ÉÁ¦
    public void SetSpringForce(float force)
    {
        springForce = force;
    }

    // ¹«¿ª·½·¨£ºÉèÖÃ×èÄáÏµÊý
    public void SetDamping(float dampingValue)
    {
        damping = dampingValue;
    }

    // ¹«¿ª·½·¨£ºÉèÖÃÄ¿±ê¶ÔÏó
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // ¹«¿ª·½·¨£ºÉèÖÃ²Ù¿Ø¶ÔÏó
    public void SetControlledObject(GameObject newControlledObject)
    {
        controlledObject = newControlledObject;
        movingRb = controlledObject.GetComponent<Rigidbody>();
        if (!movingRb)
        {
            Debug.LogError("movingRb is null!");
        }
    }
}