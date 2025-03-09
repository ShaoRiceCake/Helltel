using UnityEngine;

public class ApplyForceUpwards : MonoBehaviour
{
    [Header("Force Settings")]
    public float forceMagnitude = 10f; // 施加的力的大小
    public ForceMode forceMode = ForceMode.Force; // 力的模式

    private Rigidbody targetRigidbody; // 目标物体的 Rigidbody 组件

    void Start()
    {
        // 获取当前物体上的 Rigidbody 组件
        targetRigidbody = GetComponent<Rigidbody>();
        if (targetRigidbody == null)
        {
            Debug.LogError("No Rigidbody component found on this object!");
        }
    }

    void Update()
    {
        // 按下空格键时施加力
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ApplyForce();
        }
    }

    // 施加向上的力
    public void ApplyForce()
    {
        if (targetRigidbody == null) return;

        // 计算向上的力
        Vector3 force = Vector3.up * forceMagnitude;

        // 施加力
        targetRigidbody.AddForce(force, forceMode);
        Debug.Log("Applied force: " + force);
    }
}