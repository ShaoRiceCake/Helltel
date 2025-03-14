using UnityEngine;

public class StableBodyAndHead : MonoBehaviour
{
    public GameObject foot1; // ��1����
    public GameObject foot2; // ��2����
    public GameObject head;  // ͷ����

    [Header("�߶ȿ��Ʋ���")]
    public float baseHeight = 1.0f;      // �����߶�ƫ��
    public float logFactor = 0.5f;       // ��������ϵ��

    void Update()
    {
        Vector3 foot1Pos = foot1.transform.position;
        Vector3 foot2Pos = foot2.transform.position;

        // ����������β���
        Vector3 footVector = foot2Pos - foot1Pos;
        float footDistance = footVector.magnitude;
        Vector3 midpoint = (foot1Pos + foot2Pos) * 0.5f;
        float maxFootY = Mathf.Min(foot1Pos.y, foot2Pos.y);


        // ����ͷ��Ŀ��λ��
        Vector3 headPosition = midpoint + Vector3.up ;


        // Ӧ������λ��
        head.transform.position = headPosition;
    }
}