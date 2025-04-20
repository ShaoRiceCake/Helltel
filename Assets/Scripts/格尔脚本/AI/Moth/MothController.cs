using System.Collections;
using System.Collections.Generic;
using Helltal.Gelercat;
using UnityEngine;
using NPBehave;
using UnityEngine.PlayerLoop;
using UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers;
using NUnit.Framework;

/// <summary>
/// 集群行为状态结构体
/// </summary>
public struct BoidsState
{
    public Vector3 newVelocity; // 下一帧中的速度
    public List<MothController> neighbors; // 附近所有的虫子列表
    public List<MothController> collisionRisks; // 距离过近的虫子列表(具有碰撞风险，需要处理)
}


public class MothController : GuestBase, IHurtable
{
    [Header("这个虫子属于哪个组")]
    public MothGroupController belongToGroup; // 所属的虫群


    [Header("集群个体的状态记录")]
    private BoidsState boidsState;

    [Header("虫子最大速度")]
    public float maxVelocity = 30f; // 最大速度
    [Header("虫子最小速度")]
    public float minVelocity = 0f; // 最小速度



    private Rigidbody rb; // 刚体组件

    private Root behaviorTree;

    public void Awake()
    {
        belongToGroup.RegisterMoth(this.gameObject); // 注册虫子到虫群
    }
    protected override void Start()
    {
        base.Start();
        InitSettings();
        // this.transform.SetParent(belongToGroup.transform);
        behaviorTree = GetBehaviorTree();

#if UNITY_EDITOR
        Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
        debugger.BehaviorTree = behaviorTree;
#endif
        if (behaviorTree != null)
        {
            if (Debugging) Debug.Log("BehaviorTree is not null!");
            behaviorTree.Blackboard["UnderGroup"] = true; // 聚集状态标志
            behaviorTree.Blackboard["Dead"] = false; // 死亡标志
            behaviorTree.Blackboard["Attack"] = false;
        }
        else
        {
            if (Debugging) Debug.LogError("BehaviorTree is null!");
        }
        behaviorTree.Start();
    }

    private void InitSettings()
    {
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>(); //添加刚体组件
        rb.useGravity = false; // 不使用重力
        rb.isKinematic = false; // 取消动力学 
        boidsState = new BoidsState(); // 初始化集群个体的状态记录
        BoidInitState(); // 初始化状态
    }

    protected override Root GetBehaviorTree()
    {
        // return new Root(
        //     // new Selector(
        //     // // 死亡

        //     // // 被抓住
        //     // // 眩晕
        //     // // 攻击
        //     // // 集体行动


        //     // )
        //     new Selector(

        //     )
            
        // );

        return new Root(
            new Selector(
                BuildDeadBranch(),
                new Selector(
                    BuildGroupMoveBranch()// 集体行动
                )               

            )
        );
    }

    // //行为树实现
    // ====================== 
    private Node BuildDeadBranch()
    {
        var branch = new Condition(isDied, Stops.NONE,
            new Sequence(
                new Action(() =>
                {
                    Debug.Log("开始死亡表现！");

                }),
                new Wait(5.0f), // 表现等待时间，比如动画时长
                new Action(() =>
                {
                    Debug.Log("表现完成，执行销毁！");
                    Destroy(gameObject);
                }),
                new WaitUntilStopped() // 防止Sequence结束后重新执行
            ));
        return branch;
    }
    
    private Node BuildGroupMoveBranch()
    {
        var branch = new BlackboardCondition("UnderGroup", Operator.IS_EQUAL, true, Stops.SELF,
            new Sequence(
                new WaitUntilStopped() // 防止Sequence结束后重新执行
            ));
        return branch;
    }


    private bool isDied()
    {
        if(curHealth.Value <= 0)
        {
            behaviorTree.Blackboard["Dead"] = true; // 死亡标志
            Debug.Log("判定死亡");
            return true;
        }
        Debug.Log("判定没死");
        return false;
    }

    // 实现 IHurtable 接口
    public void TakeDamage(int damage)
    {
        // 处理伤害逻辑
        Debug.Log("Take Damage: " + damage);
    }

    protected override void Update()
    {
        base.Update();
        if (behaviorTree.Blackboard["UnderGroup"].Equals(true))
        {
            BoidUpdateState(); // 更新状态
        }

    }
    protected override void LateUpdate()
    {
        base.LateUpdate();
        if (behaviorTree.Blackboard["UnderGroup"].Equals(true))
        {
            BoidApplyState(); // 应用状态
        }
    }


    private void BoidInitState()
    {
        boidsState.newVelocity = rb.velocity; // 下一帧中的速度
        boidsState.neighbors = new List<MothController>(); // 附近所有的虫子列表
        boidsState.collisionRisks = new List<MothController>(); // 距离过近的虫子列表(具有碰撞风险，需要处理)
       
    }
    private void BoidResetState()
    {
        boidsState.newVelocity = rb.velocity; // 下一帧中的速度


    }

    // 更新下一时刻 速度矢量 
    private void BoidUpdateState()
    {
        UpdateNeighbors(); // 更新邻居状态
        boidsState.newVelocity = rb.velocity;
        // 速度匹配
        Vector3 neighborVelocity = GetAverageVelocity(); // 获取邻居的平均速度
        boidsState.newVelocity += neighborVelocity * belongToGroup.velocityMatchingAmt; // 速度匹配
        // 向心聚集
        Vector3 neighborCenterOffset = GetAveragePosition(boidsState.neighbors) - this.transform.position; // 获取邻居的平均位置
        boidsState.newVelocity += neighborCenterOffset * belongToGroup.flockCenteringAmt; // 向心聚集
        // 互斥
        Vector3 dist;
        if (boidsState.collisionRisks.Count > 0) // 处理最近的虫子列表
        {
            if(Debugging) Debug.Log("Collision Risks Count: " + boidsState.collisionRisks.Count);
            Vector3 collisionAveragePos = GetAveragePosition(boidsState.collisionRisks); // 获取最近虫子的平均位置
            dist = collisionAveragePos - this.transform.position; // 计算距离
            boidsState.newVelocity += dist * belongToGroup.collisionAvoidanceAmt; // 排斥性
        }

        // 追随目标

        Vector3 targetPos = belongToGroup.transform.position; // 目标位置
        dist = targetPos - this.transform.position; // 计算距离
        boidsState.newVelocity += dist * belongToGroup.TargetAmt; // 追随目标
    }
    private void BoidApplyState()
    {
        // 对速度进行插值平滑，避免瞬间加速或跳变
        Vector3 currentVelocity = rb.velocity;
        Vector3 targetVelocity = boidsState.newVelocity;

        // 可在群体控制器里设一个全局插值系数，如 velocityLerpAmt = 0.1f
        float lerpAmt = belongToGroup.velocityLerpAmt;
        Vector3 finalVelocity = Vector3.Lerp(currentVelocity, targetVelocity, lerpAmt);

        // 限制最大最小速度
        float speed = finalVelocity.magnitude;
        if (speed > maxVelocity)
            finalVelocity = finalVelocity.normalized * maxVelocity;
        else if (speed < minVelocity)
            finalVelocity = finalVelocity.normalized * minVelocity;

        // 设置 Rigidbody 速度，交由物理引擎驱动移动
    
 rb.velocity = finalVelocity; // 设置刚体速度
    
        this.transform.LookAt(this.transform.position + finalVelocity); // 让虫子朝向移动的方向
    }


    /// <summary>
    /// tools 负责更新boid状态
    /// </summary>
    private void UpdateNeighbors()
    {
        float closesDist = float.MaxValue;  //最小间距，MaxValue 为浮点数的最大值
        Vector3 delta;              //当前 boid 与其他某个 boid 的三维间距 
        float dist;                 //三位间距转换为的 实数间距

        boidsState.neighbors.Clear();          //清理上次表的数据
        boidsState.collisionRisks.Clear();     //清理上次表的数据

        //遍历目前所有的 boid，依据设定的范围值筛选出 附近的boid 与 最近的boid 于各自表中
        foreach (MothController b in belongToGroup.GetMothList())
        {
            if (b == this)   //跳过自身
                continue;

            delta = b.transform.position - this.transform.position;  //遍历到的 b 与当前持有的 boi(都为boid) 的三维间距
            dist = delta.magnitude;     //间距

            if (dist < closesDist)
            {
                closesDist = dist;      //更新最小间距
            }

            if (dist < belongToGroup.nearDist)  //处在附近的 boid 范围
                boidsState.neighbors.Add(b);

            if (dist < belongToGroup.collisionDist) //处在最近的 boid 范围(有碰撞风险)
                boidsState.collisionRisks.Add(b);
        }

        if (boidsState.neighbors.Count == 0)   //若没有其他满足邻近范围的boid，则将自身boid纳入附近的boid表中
            boidsState.neighbors.Add(this);
    }
    private Vector3 GetAverageVelocity()
    {
        Vector3 averageVelocity = Vector3.zero;
        foreach (MothController neighbor in boidsState.neighbors)
        {
            averageVelocity += neighbor.GetRbVelocity(); // 获取邻居的速度
        }
        averageVelocity /= boidsState.neighbors.Count;
        return averageVelocity;
    }
    private Vector3 GetAveragePosition(List<MothController> someBoids)
    {
        Vector3 averagePosition = Vector3.zero;
        foreach (MothController neighbor in someBoids)
        {
            averagePosition += neighbor.transform.position; // 获取邻居的位置
        }
        averagePosition /= someBoids.Count;
        return averagePosition;
    }


    public Vector3 GetRbVelocity()
    {
        return rb.velocity;
    }
}
