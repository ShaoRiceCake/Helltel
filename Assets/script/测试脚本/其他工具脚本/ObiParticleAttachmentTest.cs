using UnityEngine;
using Obi;

public class ObiParticleAttachmentTest : MonoBehaviour
{
    public ObiParticleAttachment obiParticleAttachment; // ���� ObiParticleAttachment ���
    public Transform targetA; // ����A
    public Transform targetB; // ����B

    void Update()
    {
        // ������������ʱ����Ŀ����Ϊ����A
        if (Input.GetMouseButtonDown(0))
        {
            if (obiParticleAttachment != null && targetA != null)
            {
                obiParticleAttachment.enabled = true;
                obiParticleAttachment.BindToTarget(targetA);
                Debug.Log("Ŀ������Ϊ����A");
            }
        }

        // ���ɿ�������ʱ����Ŀ����Ϊ����B
        if (Input.GetMouseButtonUp(0))
        {
            obiParticleAttachment.enabled = true;
            if (obiParticleAttachment != null && targetB != null)
            {
                obiParticleAttachment.BindToTarget(targetB);
                Debug.Log("Ŀ������Ϊ����B");
            }
        }

        // ����������Ҽ�ʱ�����Ŀ��
        if (Input.GetMouseButtonDown(1))
        {
            if (obiParticleAttachment != null)
            {
                obiParticleAttachment.enabled = false;
                Debug.Log("Ŀ�������");
            }
        }
    }
}