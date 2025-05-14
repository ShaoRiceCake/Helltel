using Helltal.Gelercat;
using NPBehave;
using UnityEngine;

public class MRBunnyController : GuestBase, IHurtable
{
    // Start is called before the first frame update
    [Header("MR-Bunny的移动速度")]
    public float moveSpeed = 3.5f; // 移动速度
    [Header("MR-Bunny 冲刺范围")]
    public float chaseDistance = 10f; // 冲刺范围
    [Header("MR-Bunny的追击速度")]
    public float chaseSpeed = 5f; // 追击速度
    [Header("MR-Bunny 开始攻击的范围")]
    public float attackDistance = 2f; // 攻击范围

    [Header("搜索精度")]
    public float searchAccuracy = 5f; // 搜索精度
    // public List<GameObject> EnemyList = new List<GameObject>(); //敌人列表
    public GameObject CurTarget; //当前目标    
    private Node getDamageNode;
    private Node attackNode;
    protected override void Awake()
    {
        base.Awake();

        sensor = this.gameObject.GetComponent<GuestSensor>();
        if (sensor == null)
        {
            sensor = this.gameObject.AddComponent<GuestSensor>();
        }

    }
    protected override void Start()
    {
        base.Start();
        behaviorTree = GetBehaviorTree();
#if UNITY_EDITOR
        Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
        debugger.BehaviorTree = behaviorTree;
#endif
        behaviorTree.Blackboard["getDamage"] = false; // 受伤标志
        behaviorTree.Blackboard["isDead"] = false; // 死亡标志
        behaviorTree.Blackboard["isAttacking"] = false; // 攻击标志
        behaviorTree.Start();
    }

    protected override Root GetBehaviorTree()
    {
        getDamageNode = BuildGetDamageBranch(); // 受伤被打断
        attackNode = BuildAttackBranch(); // 攻击被打断
        return new Root(
            new Selector(
                BuildDeadBranch(),
                new Selector(
                    getDamageNode,
                    attackNode, // 攻击
                    new Selector(
                        BuildChesingBranch(), // 追击
                        BuildPatrolBranch() // 巡逻 
                    )

                )

            )


        );

    }
    private Node BuildChesingBranch()
    {
        return new Condition(IsEnemyCanSee, Stops.LOWER_PRIORITY,
            new Action(() =>
            {
                if (CurTarget != null && agent.isOnNavMesh)
                {
                    float distance = Vector3.Distance(transform.position, CurTarget.transform.position);
                    if (distance <= attackDistance)
                    {
                        agent.ResetPath(); // 停止导航
                        agent.speed = 0f;
                    }
                    else
                    {
                        agent.SetDestination(CurTarget.transform.position);
                        agent.speed = (distance >= chaseDistance) ? moveSpeed : chaseSpeed;
                    }
                }
            }));
    }

    bool IsEnemyCanSee()
    {
        if (sensor != null)
        {
            if (sensor.detectedTargets.Count > 0)
            {
                foreach (var target in sensor.detectedTargets)
                {
                    // if (EnemyList.Contains(target.gameObject.transform.root.gameObject))
                    if (GameManager.instance.playerIdentifiers.Contains(target.gameObject.transform))
                    {
                        CurTarget = target.gameObject;
                        return true;
                    }
                }
                CurTarget = null; // 如果没有检测到目标，则设置为null
            }
        }
        // CurTarget = null; // 如果没有检测到目标，则设置为null
        return false;
    }
    private Node BuildPatrolBranch()
    {
        return new Condition(IsNavAgentOnNavmesh, Stops.NONE,
            new Sequence(
                new Action(
                    () =>
                    {
                        agent.speed = moveSpeed; // 设置巡逻速度
                    }
                ),

                new Repeater(
                new Cooldown(1f,
                new Patrol(agent, navPointsManager,searchAccuracy))
            ))

        );
    }
    private Node BuildDeadBranch()
    {
        var branch = new BlackboardCondition("isDead", Operator.IS_EQUAL, true, Stops.SELF,
            new Sequence(
                new Action(() =>
                {
                    presenter.SetTrigger("Die"); // 播放死亡动画
                }),

                new WaitUntilStopped() // 防止Sequence结束后重新执行
            ));
        return branch;
    }
    private Node BuildGetDamageBranch()
    {
        return new BlackboardCondition("getDamage", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
            new Sequence(
                new Action(() =>
                {
                    presenter.SetTrigger("hurt"); // 播放受伤动画


                }),
                new Action(() =>
                {
                    if (IsNavAgentOnNavmesh())
                    {
                        agent.ResetPath(); // 停止追击
                        agent.velocity = Vector3.zero; // 停止移动
                        agent.speed = 0f; // 保证站定
                    }
                }
                ),
                new WaitUntilStopped()

            ));
    }
    void OnHurtEnd()
    {
        Debug.Log("监听到：受伤结束！");
        behaviorTree.Blackboard["getDamage"] = false; // 重置受伤标志
    }
    private bool canAttack = true;
    private Node BuildAttackBranch()
    {

        return new BlackboardCondition("isAttacking", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
        new Sequence(
            new Action(() =>
            {
                agent.ResetPath();     // 停止追击
                agent.speed = 0f;      // 保证站定
                presenter.SetTrigger("Attack"); // 播放攻击动画
            }),
            new WaitUntilStopped()
        )
    );


    }
    // 监听攻击结束
    void OnAttackEnd()
    {
        Debug.Log("监听到：攻击结束！");
        behaviorTree.Blackboard["isAttacking"] = false; // 重置攻击标志
        canAttack = true; // 解除攻击锁

        if (attackNode != null && attackNode.CurrentState == Node.State.ACTIVE)
        {
            attackNode.Stop();  // 强制退出攻击节点
            Debug.Log("强制退出攻击节点！");
        }

    }
   public void TakeDamage(int damage)
    {
        if (damage >= curHealth.Value)
        {
            curHealth.Value = 0;
            Debug.Log("MR-Bunny死亡！当前血量：" + curHealth.Value);
            behaviorTree.Blackboard["isDead"] = true; // 设置死亡标志
        }
        else
        {
            curHealth.Value -= damage;
            Debug.Log("MR-Bunny受伤！当前血量：" + curHealth.Value);
            if (getDamageNode != null && getDamageNode.CurrentState == Node.State.ACTIVE)
            {
                getDamageNode.Stop();
            }
            // getDamageNode?.Stop(); // 停止受伤行为树节点
            // 重新触发受伤行为
            behaviorTree.Blackboard["getDamage"] = true;
        }

    }

    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(1);
        }


        if (CurTarget != null)
        {
            float distance = Vector3.Distance(transform.position, CurTarget.transform.position);
            if (distance <= attackDistance)
            {

                if (canAttack && !behaviorTree.Blackboard["isAttacking"].Equals(true))
                {
                    canAttack = false;
                    behaviorTree.Blackboard["isAttacking"] = true; // 手动触发攻击行为
                }
                agent.ResetPath();
                agent.speed = 0f;

            }
        }
        if (behaviorTree.Blackboard["isAttacking"].Equals(true) && behaviorTree.Blackboard["isDead"].Equals(false))
        {
            // 计算攻击目标的方向
            Vector3 targetPosition = CurTarget.transform.position;
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0; // 确保只在水平方向旋转
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
        else if (CurTarget != null && behaviorTree.Blackboard["isDead"].Equals(false))
        {
            agent.ResetPath(); // 停止追击
            agent.speed = 0f; // 保证站定

            Vector3 targetPosition = CurTarget.transform.position;
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0; // 确保只在水平方向旋转
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }
    protected override void LateUpdate()
    {
        base.LateUpdate();
        // 
        // 1. 获取当前速度
        Vector3 horizontalVelocity = agent.velocity;
        horizontalVelocity.y = 0f; // 只考虑水平速度

        

        float currentSpeed = horizontalVelocity.magnitude;
        // 2. 归一化：0 = idle，0.5 = walk，1 = run
        float normalizedSpeed = 0f;
        if (currentSpeed > 0.01f)
        {
            if (currentSpeed < moveSpeed)
            {
                // 比行走还慢，归一到 0.5 以下
                normalizedSpeed = Mathf.InverseLerp(0, moveSpeed, currentSpeed) * 0.5f;
            }
            else
            {
                // 介于 walk 和 run 之间
                normalizedSpeed = Mathf.InverseLerp(moveSpeed, chaseSpeed, currentSpeed) * 0.5f + 0.5f;
            }
        }

        // 3. 设置 Animator 参数（用于控制 Blend Tree）
        presenter.SetFloat("Speed", normalizedSpeed);
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseDistance); // 绘制追击范围
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackDistance); // 绘制攻击范围
        Gizmos.color = Color.yellow; // 绘制搜索精度范围
        Gizmos.DrawWireSphere(transform.position, searchAccuracy); // 绘制搜索精度范围

    }

    void OnDestroy()
    {
        behaviorTree?.Stop(); // 停止行为树
        base.OnDestroy();
        
    }
}
