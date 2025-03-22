using UnityEngine;
using Obi;

public class ObiParticleAttachmentTest : MonoBehaviour
{
    public ObiParticleAttachment obiParticleAttachment; // 引用 ObiParticleAttachment 组件
    public Transform targetA; // 对象A

    bool isCatching = false;

    void Update()
    {
        // 当按下鼠标左键时，将目标设为对象A
        if (Input.GetMouseButtonDown(0))
        {
            if (obiParticleAttachment != null && targetA != null && !isCatching)
            {
                obiParticleAttachment.enabled = true;
                obiParticleAttachment.BindToTarget(targetA);
                isCatching = true;
            }
        }

        // 当松开鼠标左键时，将目标设为对象B
        if (Input.GetMouseButtonUp(0))
        {
            obiParticleAttachment.enabled = false;
            isCatching = false;
        }
    }
}