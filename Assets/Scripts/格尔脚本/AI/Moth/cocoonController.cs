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
    [Header("羽衣蛾茧预制体")] public GameObject cocoonPrefab; //羽衣蛾茧预制体
    [Header("触发后孵卵时间范围")] public Vector2 hatchTimeRange = new Vector2(5f, 10f); //触发后孵卵时间范围
    
    [Header("触发范围")] public float triggerRange = 5f; //触发范围
    
    void Start()
    {
        if(this.gameObject.GetComponent<Collider>() == null)
            this.gameObject.AddComponent<SphereCollider>().isTrigger = true; //添加触发器
        else
            this.gameObject.GetComponent<Collider>().isTrigger = true; //添加触发器
        if(this.gameObject.GetComponent<GuestPresenter>() == null)
            this.gameObject.AddComponent<GuestPresenter>(); //添加表现层组件
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player")) //如果碰撞到玩家
        {
            
        }
    }
}
