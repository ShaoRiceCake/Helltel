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
    
    private GuestPresenter presenter; //表现层组件
    private Collider col; //碰撞体组件

    private bool hatchlock = false; //孵化锁
    
    void Awake()
    {
        if(this.gameObject.GetComponent<Collider>() == null)
            this.gameObject.AddComponent<SphereCollider>().isTrigger = true; //添加触发器
        else
            this.gameObject.GetComponent<Collider>().isTrigger = true; //添加触发器
        if(this.gameObject.GetComponent<GuestPresenter>() == null)
            this.gameObject.AddComponent<GuestPresenter>(); //添加表现层组件

        presenter = this.gameObject.AddComponent<GuestPresenter>(); //添加表现层组件
        col = this.gameObject.GetComponent<Collider>(); //获取碰撞体组件
        col.isTrigger = true; //设置为触发器
        
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision"); //调试信息
        if(other.gameObject.CompareTag("Player")&&!hatchlock) //如果碰撞到玩家
        {
            StartCoroutine(GenerateMoth()); //开始孵化协程
            Debug.Log("Player Enter"); //调试信息
        }
    }

    /// <summary>
    ///  孵化协程
    /// <summary>
    IEnumerator GenerateMoth()
    {
        hatchlock = true; //锁定孵化
        
        float hatchTime = Random.Range(hatchTimeRange.x, hatchTimeRange.y); //随机孵化时间

        // set clip time to hatchTime
        presenter.SetTrigger("Hatch"); //播放孵化动画
        Debug.Log("Hatch"); //调试信息
        yield return new WaitUntil(()=> presenter.GetCurrentAnimationState().IsName("Hatching")); //等待动画播放完成
        
        // float clipTime = presenter.GetCurrentAnimationCliplength(); //获取动画片段时间
        // presenter.SetAnimatiorSpeed(clipTime / hatchTime); //设置动画速度
        yield return new WaitForSeconds(hatchTime); //等待孵化时间

        presenter.SetAnimatiorSpeed(1f); //重置动画速度

        GameObject moth = Instantiate(mothPrefab, transform.position, Quaternion.identity); //生成羽衣蛾
        Destroy(this.gameObject); //销毁茧

    } 

}
