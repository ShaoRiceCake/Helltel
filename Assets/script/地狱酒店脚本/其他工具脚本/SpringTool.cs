using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class SpringTool : MonoBehaviour
{
    // 目标对象（操控对象会向目标对象移动）
    public Transform target;

    // 操控对象（受弹簧力影响的对象）
    public GameObject controlledObject;

    // 弹簧力系数
    public float springForce = 10f;

    // 阻尼系数
    public float damping = 0.6f;

    // 是否启用弹簧效果
    public bool isSpringEnabled = false;

    // 操控对象的当前速度
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
        // 计算目标对象和操控对象之间的位移
        Vector3 displacement = target.position - controlledObject.transform.position;
        float distance = displacement.magnitude;

        // 计算弹簧力
        Vector3 springForceVector = displacement.normalized * distance * springForce;

        // 计算阻尼力
        Vector3 dampingForce = -movingRb.velocity * damping;

        // 施加合力
        movingRb.AddForce(springForceVector + dampingForce);
    }

    // 公开方法：启用弹簧效果
    public void EnableSpring()
    {
        isSpringEnabled = true;
    }

    // 公开方法：禁用弹簧效果
    public void DisableSpring()
    {
        isSpringEnabled = false;
    }

    // 公开方法：设置弹簧力
    public void SetSpringForce(float force)
    {
        springForce = force;
    }

    // 公开方法：设置阻尼系数
    public void SetDamping(float dampingValue)
    {
        damping = dampingValue;
    }

    // 公开方法：设置目标对象
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // 公开方法：设置操控对象
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