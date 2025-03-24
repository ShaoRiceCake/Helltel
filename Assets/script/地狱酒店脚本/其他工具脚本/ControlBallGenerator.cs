using UnityEngine;
using Obi;

public class ControlBallGenerator : MonoBehaviour
{
    public GameObject GenerateControlBall()
    {
        // 创建一个新的球体对象
        GameObject controlBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        controlBall.name = "ControlBall";

        // 设置缩放为 0.3 倍
        controlBall.transform.localScale = Vector3.one * 0.3f;

        // 配置 Sphere Collider 为触发器
        SphereCollider sphereCollider = controlBall.GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            sphereCollider.isTrigger = true;
        }
        
        // 禁用渲染器组件
        Renderer renderer = controlBall.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
        // 添加并配置 Obi Collider
        ObiCollider obiCollider = controlBall.AddComponent<ObiCollider>();
        obiCollider.Filter = ObiUtils.MakeFilter(ObiUtils.CollideWithNothing, 0);

        // 添加并配置 Rigidbody
        Rigidbody rigidbody = controlBall.AddComponent<Rigidbody>();
        rigidbody.mass = 0.1f;
        rigidbody.useGravity = false;

        // 添加并配置 Obi Rigidbody
        ObiRigidbody obiRigidbody = controlBall.AddComponent<ObiRigidbody>();
        obiRigidbody.kinematicForParticles = true;

        return controlBall;
    }
}