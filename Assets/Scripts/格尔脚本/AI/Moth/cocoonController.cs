using System.Collections;
using System.Collections.Generic;
using Helltal.Gelercat;
using UnityEngine;

/// <summary>
/// 羽衣蛾的jian'zi
/// </summary>
public class cocoonController : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("羽衣蛾预制体")] public GameObject mothPrefab; //羽衣蛾预制体
    [Header("触发后孵卵时间范围")] public Vector2 hatchTimeRange = new Vector2(5f, 10f); //触发后孵卵时间范围
    [Header("所属虫群")] public MothGroupController mothGroup; //所属虫群
    [Header("连锁范围")] float chainDistance = 5f; //连锁范围

    public Animator animator; //动画组件
    private bool hatchlock = false; //孵化锁

    void Awake()
    {
        if (this.gameObject.GetComponent<MeshCollider>() == null)
        {
            this.gameObject.AddComponent<MeshCollider>(); //添加网格碰撞体
            this.gameObject.GetComponent<MeshCollider>().convex = true; //设置为凸包
            this.gameObject.GetComponent<MeshCollider>().isTrigger = false; //设置为非触发器
        }
        else

        {
            this.gameObject.GetComponent<MeshCollider>().convex = true; //设置为凸包
            this.gameObject.GetComponent<MeshCollider>().isTrigger = false; //设置为非触发器
        }
        if (this.gameObject.GetComponent<GuestPresenter>() == null)
            this.gameObject.AddComponent<GuestPresenter>(); //添加表现层组件



    }

    public void OnTriggerEnter(Collider other)
    {

        Debug.Log("Collision"); //调试信息
        Debug.Log(other.gameObject.name); //调试信息
        if ((other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("PlayerBodyItem")) && !hatchlock) //如果碰撞到玩家
        {
            animator.SetTrigger("Hatch"); //播放孵化动画
            // 动画状态切换
            Debug.Log("Player Enter"); //调试信息
        }
    }

    public void OnHatchEnd()
    {
        Debug.Log("Hatch End"); //调试信息
        hatch(); //调用孵化方法

    }

    public void hatch()
    {
        if (hatchlock) return; //如果孵化锁为真，返回
        hatchlock = true; //设置孵化锁为真
        MothController moth = Instantiate(mothPrefab, transform.position, Quaternion.identity).GetComponent<MothController>(); //实例化羽衣蛾预制体        
        moth.Init(mothGroup); //初始化羽衣蛾

        // 查找周围连锁范围内的其他茧
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, chainDistance);
        foreach (var hit in hitColliders)
        {
            if (hit.gameObject == this.gameObject) continue; // 忽略自己
            cocoonController otherCocoon = hit.GetComponent<cocoonController>();
            if (otherCocoon != null)
            {
                // 触发其他茧的孵化动画
                otherCocoon.animator.SetTrigger("Hatch");
            }
        }

        // 最终销毁 GameObject 本体
        Destroy(this.transform.root.gameObject, 0.5f); //销毁本体
        
    }
    void OnDrawGizmos()
    {
        // 绘制连锁范围
        Gizmos.color = Color.blue; //设置颜色为红色
        Gizmos.DrawWireSphere(transform.position, chainDistance); //绘制连锁范围
    }
}
