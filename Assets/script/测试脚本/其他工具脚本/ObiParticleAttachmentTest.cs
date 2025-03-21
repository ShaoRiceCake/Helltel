using UnityEngine;
using Obi;

public class ObiParticleAttachmentTest : MonoBehaviour
{
    public ObiParticleAttachment obiParticleAttachment; // 引用 ObiParticleAttachment 组件
    public Transform targetA; // 对象A
    public Transform targetB; // 对象B

    void Update()
    {
        // 当按下鼠标左键时，将目标设为对象A
        if (Input.GetMouseButtonDown(0))
        {
            if (obiParticleAttachment != null && targetA != null)
            {
                obiParticleAttachment.enabled = true;
                obiParticleAttachment.BindToTarget(targetA);
                Debug.Log("目标设置为对象A");
            }
        }

        // 当松开鼠标左键时，将目标设为对象B
        if (Input.GetMouseButtonUp(0))
        {
            obiParticleAttachment.enabled = true;
            if (obiParticleAttachment != null && targetB != null)
            {
                obiParticleAttachment.BindToTarget(targetB);
                Debug.Log("目标设置为对象B");
            }
        }

        // 当按下鼠标右键时，清除目标
        if (Input.GetMouseButtonDown(1))
        {
            if (obiParticleAttachment != null)
            {
                obiParticleAttachment.enabled = false;
                Debug.Log("目标已清除");
            }
        }
    }
}