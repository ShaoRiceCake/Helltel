using System.Collections.Generic;
using Helltal.Gelercat;
using UnityEngine;
using NPBehave;
using System.Linq;
using UnityEngine.Events;

/// <summary>
/// 集群行为状态结构体
/// </summary>
public struct BoidsState
{
    public Vector3 newVelocity; // 下一帧中的速度
    public List<MothController> neighbors; // 附近所有的虫子列表
    public List<MothController> collisionRisks; // 距离过近的虫子列表(具有碰撞风险，需要处理)
}

public enum MothState
{
    UnderGroup, // 集体行动
    PrepareAttack, // 准备攻击
    Dash, // 冲刺
    Attached, // 附着
    Dead, // 死亡
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

    [Header("虫子攻击距离")]
    public float attackDistance = 5f; // 攻击距离
    [Header("虫子冲刺速度")]
    public float dashSpeed = 10f; // 冲刺速度

    [Header("虫子抱脸伤害")]
    public int attachDamage = 2; // 附着伤害
    [Header("虫子冲刺伤害")]
    public int dashDamage = 2; // 冲刺伤害


    private Vector3 dashTarget; // 冲刺目标位置



    private Rigidbody rb; // 刚体组件
    private ConfigurableJoint joint; // 关节组件


    protected override bool ShouldUseNavMeshAgent()
    {
        return false; // 禁用导航代理
    }
    protected override void Awake()
    {
        base.Awake();
        belongToGroup.RegisterMoth(this.gameObject); // 注册虫子到虫群
        InitSettings();
    }
    protected override void Start()
    {
        base.Start();

        // this.transform.SetParent(belongToGroup.transform);
        behaviorTree = GetBehaviorTree();

#if UNITY_EDITOR
        Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
        debugger.BehaviorTree = behaviorTree;
#endif
        behaviorTree.Blackboard["getDamage"] = false; // 受伤标志
        behaviorTree.Blackboard["Stunning"] = false;  // 眩晕
        behaviorTree.Blackboard["isGrabbed"] = false; // 抓取状态
        SetMothState(MothState.UnderGroup);
        behaviorTree.Start();
    }




    private void InitSettings()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = this.gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false; // 不使用重力
        rb.isKinematic = false; // 取消动力学         

        joint = this.gameObject.GetComponent<ConfigurableJoint>();
        if (joint == null)
        {
            joint = this.gameObject.AddComponent<ConfigurableJoint>();
        }



        boidsState = new BoidsState(); // 初始化集群个体的状态记录
        BoidInitState(); // 初始化状态


    }
    private void Configurablejointinit(Rigidbody targetRB)
    {
        if(joint == null)
        {
            joint = this.gameObject.AddComponent<ConfigurableJoint>();
        }
        joint.connectedBody = targetRB;
        joint.autoConfigureConnectedAnchor = false; // 不自动配置连接锚点
        joint.connectedAnchor = Vector3.zero; // 连接锚点位置
        joint.anchor = localAttachOffset; // 本地锚点位置

        joint.xMotion = ConfigurableJointMotion.Limited; // 锁定X轴运动
        joint.yMotion = ConfigurableJointMotion.Limited; // 锁定Y轴运动
        joint.zMotion = ConfigurableJointMotion.Limited; // 锁定Z轴运动

        joint.angularXMotion = ConfigurableJointMotion.Limited; // 锁定X轴角度运动
        joint.angularYMotion = ConfigurableJointMotion.Limited; // 锁定Y轴角度运动
        joint.angularZMotion = ConfigurableJointMotion.Limited; // 锁定Z轴角度运动

        JointDrive springDrive = new JointDrive
        {
            positionSpring = 1000f, // 弹簧力
            positionDamper = 10f, // 阻尼力
            maximumForce = Mathf.Infinity // 最大力
        };
        joint.xDrive = joint.yDrive = joint.zDrive = springDrive; // 设置X、Y、Z轴的驱动
        // joint.enableCollision = false;
        // joint.enablePreprocessing = true;

        // joint.en
    }

    protected override Root GetBehaviorTree()
    {
        return new Root(
            new Selector(
                BuildDeadBranch(),
                new Selector(
                BuildGetDamageBranch(), // 受伤表现
                BuildStunnedBranch(),   // 眩晕表现
                BuildBeCatchedBranch(), // 被抓取表现
                BuildAttachingBranch(), // 贴脸               
                BuildAttackBranch(),    // 攻击
                BuildGroupMoveBranch()  // 集体行动
                )

            )


        );
    }

    // //行为树实现
    // ====================== 
    private Node BuildDeadBranch()
    {
        var branch = new BlackboardCondition("State", Operator.IS_EQUAL, MothState.Dead, Stops.SELF,
            new Sequence(
                new Action(() =>
                {
                    //给一个向后的速度， debug用
                    rb.AddForce(Vector3.back * 10f, ForceMode.Impulse); // 向后弹起
                    Debug.Log("开始死亡表现！");
                    rb.useGravity = true;
                }),
                new Wait(5.0f), // 表现等待时间，动画时长
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
        var branch =
            new Sequence(
                new Action(() =>
                {
                    SetMothState(MothState.UnderGroup); // 设置虫子状态为集体行动
                }),
                new WaitUntilStopped()
            );
        return branch;
    }


    private Node BuildAttackBranch()
    {
        var branch = new Condition(TryAttack, Stops.LOWER_PRIORITY,
           new Sequence(
                new Action(() =>
                {
                    List<BodyAnchor> anchors = belongToGroup.CurTarget?.transform.root.GetComponentsInChildren<BodyAnchor>().ToList(); // 获取所有的锚点

                    if (anchors == null || anchors.Count == 0) return;
                    Transform face = anchors?.Find(x => x.Name == "HeadBall")?.transform; // 找到名为"HeadBall"的锚点
                    if (face == null) return;
                    dashTarget = face.position; // 获取目标位置
                }),
                new Selector(
                    new Condition(() => dashTarget != null, Stops.LOWER_PRIORITY_IMMEDIATE_RESTART,
                        new Sequence(
                            new Action(() =>
                            { SetMothState(MothState.Dash); } // 设置虫子状态为冲刺
                            ),
                            new Wait(1f), // 前摇时间
                            new Action(() =>
                            {
                                Vector3 dir = (dashTarget - this.transform.position).normalized; // 计算冲刺方向
                                rb.AddForce(dir * 10f, ForceMode.Impulse); // 冲刺
                            })
                    )

                )

            )));
        return branch;
    }

    private Node BuildAttachingBranch()
    {
        return new BlackboardCondition("State", Operator.IS_EQUAL, MothState.Attached.ToString(), Stops.LOWER_PRIORITY_IMMEDIATE_RESTART,
            new Service(
                3.0f, // 每 1 秒执行一次
                () =>
                {
                    if (belongToGroup.CurTarget != null)
                    {
                        Debug.Log("虫子附着在玩家身上, 每秒造成伤害");
                        GameController.Instance.DeductHealth(attachDamage); // 扣除玩家血量
                    }
                },
                new WaitUntilStopped()
            )
        );
    }

    private Node BuildGetDamageBranch()
    {
        return new BlackboardCondition("getDamage", Operator.IS_EQUAL, true, Stops.LOWER_PRIORITY,
            new Sequence(
                new Action(() =>
                {
                    Debug.Log("受伤表现！");
                    // 给一个向上的速度， debug用
                    rb.AddForce(Vector3.up * 20f, ForceMode.Impulse); // 向上弹起

                }),
                new Wait(1.0f), // 受伤表现等待时间
                new Action(() =>
                {
                    Debug.Log("表现完成！");
                    behaviorTree.Blackboard["getDamage"] = false; // 重置受伤标志
                })
            ));
    }

    private Node BuildBeCatchedBranch()
    {
        return new BlackboardCondition("isGrabbed", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
            new Sequence(
                new Action(() =>
                {
                    Debug.Log("虫子被抓取了！");
                    rb.velocity = Vector3.zero; // 停止虫子移动
                }),
                new WaitUntilStopped()
            ));
    }
    public void Grabb_HandleGrabb()
    {
        if (behaviorTree.Blackboard["State"].Equals(MothState.Attached.ToString()))
        {
            DetachFromTarget(); // 解除附着

        }
        behaviorTree.Blackboard["isGrabbed"] = true; // 设置抓取状态
    }
    public void Grabb_HandleRelease()
    {
        behaviorTree.Blackboard["isGrabbed"] = false; // 设置抓取状态
        wasGrabbed = true; // 记录抓取状态
    }
    private bool wasGrabbed = false; // 记录曾经被抓取过，准备判断第一次落地
    private float stunThresholdSpeed = 5.0f; // 眩晕速度阈值
    private Node BuildStunnedBranch()
    {
        return new BlackboardCondition("Stunning", Operator.IS_EQUAL, true, Stops.LOWER_PRIORITY,
            new Sequence(
                new Action(() =>
                {
                    Debug.Log("眩晕");
                    rb.useGravity = true;
                }),
                new Wait(3f),
                new Action(() =>
                {
                    Debug.Log("Ending stunned");
                    rb.useGravity = false;
                    behaviorTree.Blackboard["Stunning"] = false;
                })
            )
        );
    }

    // 实现 IHurtable 接口
    public void TakeDamage(int damage)
    {
        // 处理伤害逻辑
        if (damage >= curHealth.Value)
        {
            curHealth.Value = 0;
            SetMothState(MothState.Dead); // 设置虫子状态为死亡
        }
        else
        {
            curHealth.Value -= damage;
            Debug.Log("虫子受伤了！当前血量：" + curHealth.Value);
            behaviorTree.Blackboard["getDamage"] = true; // 受伤标志
        }
    }


    protected override void Update()
    {
        base.Update();
        if (!DEBUG_STOP_BEHAVIOR_TREE)
        {
            if (behaviorTree.Blackboard["State"].Equals(MothState.UnderGroup.ToString()))
            {
                BoidUpdateState(); // 更新状态
            }
            if (behaviorTree.Blackboard["State"].Equals(MothState.Dash.ToString()))
            {
            }

            if (behaviorTree.Blackboard["State"].Equals(MothState.Attached.ToString()))
            {
                if (attachTarget != null)
                {

                }

            }
        }
    }
    protected override void LateUpdate()
    {
        base.LateUpdate();
        if (!DEBUG_STOP_BEHAVIOR_TREE)
        {
            if (behaviorTree.Blackboard["State"].Equals(MothState.UnderGroup.ToString()))
            {
                BoidApplyState(); // 应用状态
            }

        }

    }

    // 虫子集群行为++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    private void BoidInitState()
    {
        boidsState.newVelocity = rb.velocity; // 下一帧中的速度
        boidsState.neighbors = new List<MothController>(); // 附近所有的虫子列表
        boidsState.collisionRisks = new List<MothController>(); // 距离过近的虫子列表(具有碰撞风险，需要处理)

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
    //  虫子集群行为++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++



    // 攻击相关================================================================================================

    private bool TryAttack()
    {
        if (!behaviorTree.Blackboard["State"].Equals(MothState.UnderGroup.ToString()))
        {
            return false;
        }
        if (belongToGroup.CurTarget != null)
        {
            if (Vector3.Distance(this.transform.position, belongToGroup.CurTarget.transform.position) <= attackDistance)
            {
                return true; // 在攻击范围内
            }
        }

        return false;
    }

    /// <summary>
    /// 接触到玩家，附着在其身上
    /// </summary>
    /// <returns></returns>

    private Transform attachTarget; // 附着对象
    private Vector3 localAttachOffset; // 相对于玩家的位置（局部空间）
    private Quaternion localAttachRotation; // 附着时的相对朝向

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("虫子碰撞了物体！" + collision.gameObject.name);
        if ((collision.gameObject.CompareTag("Player")||collision.gameObject.CompareTag("PlayerBodyItem")) && behaviorTree.Blackboard["State"].Equals(MothState.Dash.ToString()))
            if (belongToGroup.attachingMoth == null)
            {
                // // 
                // Debug.Log("触发附着！");
                // belongToGroup.attachingMoth = this.gameObject; // 设置当前虫子为附着虫子
                // // ContactPoint contact = collision.contacts[0];
                // // Vector3 worldAttachPoint = contact.point;
                // // Vector3 normal = contact.normal;

                // // // 计算向上的投影方向
                // // Vector3 upDirection = Vector3.ProjectOnPlane(Vector3.up, normal).normalized;
                // // Quaternion attachRotation = Quaternion.LookRotation(upDirection, normal);

                // // // 保存附着信息
                // attachTarget = collision.transform;
                // // localAttachOffset = attachTarget.InverseTransformPoint(worldAttachPoint);
                // // localAttachRotation = Quaternion.Inverse(attachTarget.rotation) * attachRotation;

                // // rb.velocity = Vector3.zero;
                // // rb.isKinematic = true;

                // // this.transform.SetParent(attachTarget); // 设置虫子为玩家的子物体

                // // SetMothState(MothState.Attached);

                belongToGroup.attachingMoth = this.gameObject; // 设置当前虫子为附着虫子
                attachTarget = collision.transform;
                List<BodyAnchor> anchors = belongToGroup.CurTarget?.transform.root.GetComponentsInChildren<BodyAnchor>().ToList(); // 获取所有的锚点
                if (anchors == null || anchors.Count == 0) return;
                Transform body = anchors?.Find(x => x.Name == "bodyCenter")?.transform; // 找到名为"bodyCenter"的锚点
                if (body == null) return;
                // 计算附着点的偏移
                ContactPoint contact = collision.contacts[0];
                Vector3 worldAttachPoint = contact.point;
                localAttachOffset = transform.InverseTransformPoint(worldAttachPoint);

                // 初始化关节连接
                Rigidbody targetRB = body.GetComponent<Rigidbody>();
                if (targetRB == null) return;

                Configurablejointinit(targetRB); // 初始化关节连接

                // 停止移动并禁用重力
                rb.velocity = Vector3.zero;
                rb.useGravity = false;

                // 切换为附着状态
                SetMothState(MothState.Attached);
            }
            else
            {
                Debug.Log("已有附着，造成伤害后眩晕");
                // take damage;
                GameController.Instance.DeductHealth(dashDamage);

                rb.AddForce(Vector3.back * 2f, ForceMode.Impulse);
                behaviorTree.Blackboard["Stunning"] = true;

            }
        if (wasGrabbed)
        {
            float impactSpeed = collision.relativeVelocity.magnitude; // 碰撞时相对速度
            Debug.Log($"释放后首次碰撞，速度 {impactSpeed}");

            if (impactSpeed >= stunThresholdSpeed)
            {
                Debug.Log("速度超过阈值，进入眩晕状态！");
                behaviorTree.Blackboard["Stunning"] = true;
            }

            wasGrabbed = false; // 无论如何，第一次碰撞后取消等待状态
        }

    }

    private void DetachFromTarget()
    {
        if (attachTarget != null)
        {
            Destroy(joint); // 销毁关节连接
            belongToGroup.attachingMoth = null; // 清除附着虫子
            rb.isKinematic = false;         // 恢复物理
            rb.useGravity = true;           // 可选：恢复重力
    
            attachTarget = null;            // 清除附着对象
            SetMothState(MothState.UnderGroup); // 回到集体行动状态或其他状态

        }
    }

    // 攻击i相关 ================================================================================================


    public Vector3 GetRbVelocity()
    {
        return rb.velocity;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        belongToGroup.UnregisterMoth(this.gameObject);
    }
    public void SetMothState(MothState state)
    {
        behaviorTree.Blackboard["State"] = state.ToString();
    }




    private void OnDrawGizmos()
    {
        // 画出攻击距离，球
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(this.transform.position, attackDistance); // 画出攻击范围
        if (Application.isPlaying && behaviorTree.Blackboard["State"].Equals(MothState.Dash.ToString()) && dashTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(dashTarget, 0.2f); // 画出冲刺目标范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(this.transform.position, dashTarget); // 画出冲刺目标

        }

    }
}
