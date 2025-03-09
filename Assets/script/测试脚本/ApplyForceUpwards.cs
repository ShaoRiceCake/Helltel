using UnityEngine;

public class ApplyForceUpwards : MonoBehaviour
{
    [Header("Force Settings")]
    public float forceMagnitude = 10f; // ʩ�ӵ����Ĵ�С
    public ForceMode forceMode = ForceMode.Force; // ����ģʽ

    private Rigidbody targetRigidbody; // Ŀ������� Rigidbody ���

    void Start()
    {
        // ��ȡ��ǰ�����ϵ� Rigidbody ���
        targetRigidbody = GetComponent<Rigidbody>();
        if (targetRigidbody == null)
        {
            Debug.LogError("No Rigidbody component found on this object!");
        }
    }

    void Update()
    {
        // ���¿ո��ʱʩ����
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ApplyForce();
        }
    }

    // ʩ�����ϵ���
    public void ApplyForce()
    {
        if (targetRigidbody == null) return;

        // �������ϵ���
        Vector3 force = Vector3.up * forceMagnitude;

        // ʩ����
        targetRigidbody.AddForce(force, forceMode);
        Debug.Log("Applied force: " + force);
    }
}