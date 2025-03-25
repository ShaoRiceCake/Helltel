using UnityEngine;
using Obi;

public class ControlBallGenerator : MonoBehaviour
{
    public GameObject GenerateControlBall()
    {
        // 创建一个新的球体对象
        var controlBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        controlBall.name = "ControlBall";

        // 设置缩放为 0.3 倍
        controlBall.transform.localScale = Vector3.one * 1.0f;

        // 配置 Sphere Collider 为触发器
        var sphereCollider = controlBall.GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            sphereCollider.isTrigger = true;
        }
        
        // // 禁用渲染器组件
        // Renderer renderer = controlBall.GetComponent<Renderer>();
        // if (renderer != null)
        // {
        //     renderer.enabled = false;
        // }
        // 添加并配置 Obi Collider
        var obiCollider = controlBall.AddComponent<ObiCollider>();
        obiCollider.Filter = ObiUtils.MakeFilter(ObiUtils.CollideWithNothing, 0);

        // 添加并配置 Rigidbody
        var rigidbody = controlBall.AddComponent<Rigidbody>();
        rigidbody.mass = 1f;
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        // 添加并配置 Obi Rigidbody
        var obiRigidbody = controlBall.AddComponent<ObiRigidbody>();
        obiRigidbody.kinematicForParticles = true;

        return controlBall;
    }
}