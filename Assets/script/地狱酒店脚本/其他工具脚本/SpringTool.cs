using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class SpringTool : MonoBehaviour
{
    // Ŀ����󣨲ٿض������Ŀ������ƶ���
    public Transform target;

    // �ٿض����ܵ�����Ӱ��Ķ���
    public GameObject controlledObject;

    // ������ϵ��
    public float springForce = 10f;

    // ����ϵ��
    public float damping = 0.6f;

    // �Ƿ����õ���Ч��
    public bool isSpringEnabled = false;

    // �ٿض���ĵ�ǰ�ٶ�
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
        // ����Ŀ�����Ͳٿض���֮���λ��
        Vector3 displacement = target.position - controlledObject.transform.position;
        float distance = displacement.magnitude;

        // ���㵯����
        Vector3 springForceVector = displacement.normalized * distance * springForce;

        // ����������
        Vector3 dampingForce = -movingRb.velocity * damping;

        // ʩ�Ӻ���
        movingRb.AddForce(springForceVector + dampingForce);
    }

    // �������������õ���Ч��
    public void EnableSpring()
    {
        isSpringEnabled = true;
    }

    // �������������õ���Ч��
    public void DisableSpring()
    {
        isSpringEnabled = false;
    }

    // �������������õ�����
    public void SetSpringForce(float force)
    {
        springForce = force;
    }

    // ������������������ϵ��
    public void SetDamping(float dampingValue)
    {
        damping = dampingValue;
    }

    // ��������������Ŀ�����
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // �������������òٿض���
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