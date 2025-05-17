using System.Collections.Generic;
using Helltal.Gelercat;
using UnityEngine;
using NPBehave;
using System.Linq;


/// <summary>
/// 集群行为状态结构体
/// </summary>
public struct BoidsState
{
    public Vector3 NewVelocity; // 下一帧中的速度
    public List<MothController> Neighbors; // 附近所有的虫子列表
    public List<MothController> CollisionRisks; // 距离过近的虫子列表(具有碰撞风险，需要处理)
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
    private BoidsState _boidsState;

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
    
    private Vector3 _dashTarget; // 冲刺目标位置



    private Rigidbody _rb; // 刚体组件
    private ConfigurableJoint _joint; // 关节组件


    protected override bool ShouldUseNavMeshAgent()
    {
        return false; // 禁用导航代理
    }
    public void Init(MothGroupController group)
    {
        belongToGroup = group;
        group.RegisterMoth(this.gameObject); // 注册到组
        
    }

    protected override void Awake()
    {
        base.Awake();
        InitSettings();
    }
    protected override void Start()
    {
        base.Start();

        // this.transform.SetParent(belongToGroup.transform);
        BehaviorTree = GetBehaviorTree();

#if UNITY_EDITOR
        Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
        debugger.BehaviorTree = BehaviorTree;
#endif
        BehaviorTree.Blackboard["getDamage"] = false; // 受伤标志
        BehaviorTree.Blackboard["Stunning"] = false;  // 眩晕
        BehaviorTree.Blackboard["isGrabbed"] = false; // 抓取状态
        SetMothState(MothState.UnderGroup);
        BehaviorTree.Start();
    }




    private void InitSettings()
    {
        _rb = this.gameObject.GetComponent<Rigidbody>();
        if (_rb == null)
        {
            _rb = this.gameObject.AddComponent<Rigidbody>();
        }
        _rb.useGravity = false; // 不使用重力
        _rb.isKinematic = false; // 取消动力学         

        _joint = this.gameObject.GetComponent<ConfigurableJoint>();
        if (_joint == null)
        {
            _joint = this.gameObject.AddComponent<ConfigurableJoint>();
        }



        _boidsState = new BoidsState(); // 初始化集群个体的状态记录
        BoidInitState(); // 初始化状态


    }
    private void Configurablejointinit(Rigidbody targetRB)
    {
        if (_joint == null)
        {
            _joint = this.gameObject.AddComponent<ConfigurableJoint>();
        }
        _joint.connectedBody = targetRB;
        _joint.autoConfigureConnectedAnchor = false; // 不自动配置连接锚点
        _joint.connectedAnchor = Vector3.zero; // 连接锚点位置
        _joint.anchor = _localAttachOffset; // 本地锚点位置

        _joint.xMotion = ConfigurableJointMotion.Limited; // 锁定X轴运动
        _joint.yMotion = ConfigurableJointMotion.Limited; // 锁定Y轴运动
        _joint.zMotion = ConfigurableJointMotion.Limited; // 锁定Z轴运动

        _joint.angularXMotion = ConfigurableJointMotion.Limited; // 锁定X轴角度运动
        _joint.angularYMotion = ConfigurableJointMotion.Limited; // 锁定Y轴角度运动
        _joint.angularZMotion = ConfigurableJointMotion.Limited; // 锁定Z轴角度运动

        JointDrive springDrive = new JointDrive
        {
            positionSpring = 1000f, // 弹簧力
            positionDamper = 10f, // 阻尼力
            maximumForce = Mathf.Infinity // 最大力
        };
        _joint.xDrive = _joint.yDrive = _joint.zDrive = springDrive; // 设置X、Y、Z轴的驱动
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
                    _rb.AddForce(Vector3.back * 10f, ForceMode.Impulse); // 向后弹起
                    Debug.Log("开始死亡表现！");
                    _rb.useGravity = true;
                    IsDead = true;
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
                    var anchors = belongToGroup._curTarget?.transform.root.GetComponentsInChildren<BodyAnchor>().ToList(); // 获取所有的锚点

                    if (anchors == null || anchors.Count == 0) return;
                    var face = anchors?.Find(x => x.Name == "HeadBall")?.transform; // 找到名为"HeadBall"的锚点
                    if (!face) return;
                    _dashTarget = face.position; // 获取目标位置
                }),
                new Selector(
                    new Condition(() => _dashTarget != null, Stops.LOWER_PRIORITY_IMMEDIATE_RESTART,
                        new Sequence(
                            new Action(() =>
                            { SetMothState(MothState.Dash); } // 设置虫子状态为冲刺
                            ),
                            new Wait(1f), // 前摇时间
                            new Action(() =>
                            {
                                var dir = (_dashTarget - this.transform.position).normalized; // 计算冲刺方向
                                _rb.AddForce(dir * 10f, ForceMode.Impulse); // 冲刺
                            })
                    )

                )

            )));
        return branch;
    }

    private Node BuildAttachingBranch()
    {
        return new BlackboardCondition("State", Operator.IS_EQUAL, MothState.Attached.ToString(), Stops.LOWER_PRIORITY_IMMEDIATE_RESTART,
            new Sequence(
            new Action(() =>
            {
                presenter.SetTrigger("attack"); // 播放附着动画
            }),
            new Service(
                3.0f, // 每 1 秒执行一次
                () =>
                {
                    if (!belongToGroup._curTarget) return;
                    GameController.Instance.DeductHealth(attachDamage); // 扣除玩家血量
                },
                new WaitUntilStopped()
            ))
        );
    }

    private Node BuildGetDamageBranch()
    {
        return new BlackboardCondition("getDamage", Operator.IS_EQUAL, true, Stops.LOWER_PRIORITY,
            new Sequence(
                new Action(() =>
                {
                    _rb.AddForce(Vector3.up * 20f, ForceMode.Impulse); // 向上弹起

                }),
                new Wait(1.0f), // 受伤表现等待时间
                new Action(() =>
                {
                    BehaviorTree.Blackboard["getDamage"] = false; // 重置受伤标志
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
                    _rb.velocity = Vector3.zero; // 停止虫子移动
                }),
                new WaitUntilStopped()
            ));
    }
    public void Grabb_HandleGrabb()
    {
        if (BehaviorTree.Blackboard["State"].Equals(MothState.Attached.ToString()))
        {
            DetachFromTarget(); // 解除附着

        }
        BehaviorTree.Blackboard["isGrabbed"] = true; // 设置抓取状态
    }
    public void Grabb_HandleRelease()
    {
        BehaviorTree.Blackboard["isGrabbed"] = false; // 设置抓取状态
        _wasGrabbed = true; // 记录抓取状态
    }
    private bool _wasGrabbed = false; // 记录曾经被抓取过，准备判断第一次落地
    private const float StunThresholdSpeed = 5.0f; // 眩晕速度阈值

    private Node BuildStunnedBranch()
    {
        return new BlackboardCondition("Stunning", Operator.IS_EQUAL, true, Stops.LOWER_PRIORITY,
            new Sequence(
                new Action(() =>
                {
                    Debug.Log("眩晕");
                    _rb.useGravity = true;
                }),
                new Wait(3f),
                new Action(() =>
                {
                    Debug.Log("Ending stunned");
                    _rb.useGravity = false;
                    BehaviorTree.Blackboard["Stunning"] = false;
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
            BehaviorTree.Blackboard["getDamage"] = true; // 受伤标志
        }
    }


    protected override void Update()
    {
        base.Update();
        if (!debugStopBehaviorTree)
        {
            if (BehaviorTree.Blackboard["State"].Equals(MothState.UnderGroup.ToString()))
            {
                BoidUpdateState(); // 更新状态
            }
            if (BehaviorTree.Blackboard["State"].Equals(MothState.Dash.ToString()))
            {
            }

            if (BehaviorTree.Blackboard["State"].Equals(MothState.Attached.ToString()))
            {
                if (_attachTarget != null)
                {

                }

            }
        }
    }
    protected override void LateUpdate()
    {
        base.LateUpdate();
        if (!debugStopBehaviorTree)
        {
            if (BehaviorTree.Blackboard["State"].Equals(MothState.UnderGroup.ToString()))
            {
                BoidApplyState(); // 应用状态
            }

        }

    }

    // 虫子集群行为++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    private void BoidInitState()
    {
        _boidsState.NewVelocity = _rb.velocity; // 下一帧中的速度
        _boidsState.Neighbors = new List<MothController>(); // 附近所有的虫子列表
        _boidsState.CollisionRisks = new List<MothController>(); // 距离过近的虫子列表(具有碰撞风险，需要处理)

    }


    // 更新下一时刻 速度矢量 
    private void BoidUpdateState()
    {
        UpdateNeighbors(); // 更新邻居状态
        _boidsState.NewVelocity = _rb.velocity;
        // 速度匹配
        Vector3 neighborVelocity = GetAverageVelocity(); // 获取邻居的平均速度
        _boidsState.NewVelocity += neighborVelocity * belongToGroup.velocityMatchingAmt; // 速度匹配
        // 向心聚集
        Vector3 neighborCenterOffset = GetAveragePosition(_boidsState.Neighbors) - this.transform.position; // 获取邻居的平均位置
        _boidsState.NewVelocity += neighborCenterOffset * belongToGroup.flockCenteringAmt; // 向心聚集
        // 互斥
        Vector3 dist;
        if (_boidsState.CollisionRisks.Count > 0) // 处理最近的虫子列表
        {
            Vector3 collisionAveragePos = GetAveragePosition(_boidsState.CollisionRisks); // 获取最近虫子的平均位置
            dist = collisionAveragePos - this.transform.position; // 计算距离
            _boidsState.NewVelocity += dist * belongToGroup.collisionAvoidanceAmt; // 排斥性
        }

        // 追随目标

        Vector3 targetPos = belongToGroup.transform.position; // 目标位置
        dist = targetPos - this.transform.position; // 计算距离
        _boidsState.NewVelocity += dist * belongToGroup.targetAmt; // 追随目标
    }
    private void BoidApplyState()
    {
        // 对速度进行插值平滑，避免瞬间加速或跳变
        Vector3 currentVelocity = _rb.velocity;
        Vector3 targetVelocity = _boidsState.NewVelocity;

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

        _rb.velocity = finalVelocity; // 设置刚体速度

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

        _boidsState.Neighbors.Clear();          //清理上次表的数据
        _boidsState.CollisionRisks.Clear();     //清理上次表的数据

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
                _boidsState.Neighbors.Add(b);

            if (dist < belongToGroup.collisionDist) //处在最近的 boid 范围(有碰撞风险)
                _boidsState.CollisionRisks.Add(b);
        }

        if (_boidsState.Neighbors.Count == 0)   //若没有其他满足邻近范围的boid，则将自身boid纳入附近的boid表中
            _boidsState.Neighbors.Add(this);
    }
    private Vector3 GetAverageVelocity()
    {
        Vector3 averageVelocity = Vector3.zero;
        foreach (MothController neighbor in _boidsState.Neighbors)
        {
            averageVelocity += neighbor.GetRbVelocity(); // 获取邻居的速度
        }
        averageVelocity /= _boidsState.Neighbors.Count;
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
        if (!BehaviorTree.Blackboard["State"].Equals(MothState.UnderGroup.ToString()))
        {
            return false;
        }

        if (!belongToGroup._curTarget) return false;
        return Vector3.Distance(this.transform.position, belongToGroup._curTarget.transform.position) <= attackDistance;
        // 在攻击范围内
    }

    /// <summary>
    /// 接触到玩家，附着在其身上
    /// </summary>
    /// <returns></returns>

    private Transform _attachTarget; // 附着对象
    private Vector3 _localAttachOffset; // 相对于玩家的位置（局部空间）
    private Quaternion _localAttachRotation; // 附着时的相对朝向

    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("PlayerBodyItem")) && BehaviorTree.Blackboard["State"].Equals(MothState.Dash.ToString()))
            if (!belongToGroup.attachingMoth)
            {
                belongToGroup.attachingMoth = this.gameObject; // 设置当前虫子为附着虫子
                _attachTarget = collision.transform;
                var anchors = belongToGroup._curTarget?.transform.root.GetComponentsInChildren<BodyAnchor>().ToList(); // 获取所有的锚点
                if (anchors == null || anchors.Count == 0) return;
                var body = anchors?.Find(x => x.Name == "bodyCenter")?.transform; // 找到名为"bodyCenter"的锚点
                if (!body) return;
                // 计算附着点的偏移
                var contact = collision.contacts[0];
                var worldAttachPoint = contact.point;
                _localAttachOffset = transform.InverseTransformPoint(worldAttachPoint);

                // 初始化关节连接
                var targetRb = body.GetComponent<Rigidbody>();
                if (!targetRb) return;

                Configurablejointinit(targetRb); // 初始化关节连接

                // 停止移动并禁用重力
                _rb.velocity = Vector3.zero;
                _rb.useGravity = false;

                // 切换为附着状态
                SetMothState(MothState.Attached);
            }
            else
            {
                Debug.Log("已有附着，造成伤害后眩晕");
                // take damage;
                GameController.Instance.DeductHealth(dashDamage);

                _rb.AddForce(Vector3.back * 2f, ForceMode.Impulse);
                BehaviorTree.Blackboard["Stunning"] = true;

            }

        if (!_wasGrabbed) return;
        var impactSpeed = collision.relativeVelocity.magnitude; // 碰撞时相对速度

        if (impactSpeed >= StunThresholdSpeed)
        {
            BehaviorTree.Blackboard["Stunning"] = true;
        }

        _wasGrabbed = false; // 无论如何，第一次碰撞后取消等待状态

    }

    private void DetachFromTarget()
    {
        if (!_attachTarget) return;
        Destroy(_joint); // 销毁关节连接
        belongToGroup.attachingMoth = null; // 清除附着虫子
        _rb.isKinematic = false;         // 恢复物理
        _rb.useGravity = true;           // 可选：恢复重力

        _attachTarget = null;            // 清除附着对象
        SetMothState(MothState.UnderGroup); // 回到集体行动状态或其他状态
    }

    // 攻击i相关 ================================================================================================
    public Vector3 GetRbVelocity()
    {
        return _rb.velocity;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        belongToGroup.UnregisterMoth(this.gameObject);
    }

    public void SetMothState(MothState state)
    {
        BehaviorTree.Blackboard["State"] = state.ToString();
    }
}
