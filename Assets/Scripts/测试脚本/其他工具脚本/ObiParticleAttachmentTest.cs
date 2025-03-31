using UnityEngine;
using Obi;

public class ObiParticleAttachmentTest : MonoBehaviour
{
    public ObiParticleAttachment obiParticleAttachment; // ���� ObiParticleAttachment ���
    public Transform targetA; // ����A

    bool isCatching = false;

    void Update()
    {
        // ������������ʱ����Ŀ����Ϊ����A
        if (Input.GetMouseButtonDown(0))
        {
            if (obiParticleAttachment != null && targetA != null && !isCatching)
            {
                obiParticleAttachment.enabled = true;
                obiParticleAttachment.BindToTarget(targetA);
                isCatching = true;
            }
        }

        // ���ɿ�������ʱ����Ŀ����Ϊ����B
        if (Input.GetMouseButtonUp(0))
        {
            obiParticleAttachment.enabled = false;
            isCatching = false;
        }
    }
}