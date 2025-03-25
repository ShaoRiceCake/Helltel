using UnityEngine;
using Obi;

public class ControlBallGenerator : MonoBehaviour
{
    public static GameObject GenerateControlBall()
    {
        // 创建一个新的球体对象
        var controlBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        controlBall.name = "ControlBall";
        controlBall.tag = "Uncatchable";
        
        // 设置缩放为 0.3 倍
        controlBall.transform.localScale = Vector3.one * 0.5f;

        // 配置 Sphere Collider 为触发器
        var sphereCollider = controlBall.GetComponent<SphereCollider>();
        if (!sphereCollider)
        {
        }
        else
        {
            sphereCollider = controlBall.AddComponent<SphereCollider>();
        }

        sphereCollider.isTrigger = true;

        // 禁用渲染器组件
        var renderer = controlBall.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
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