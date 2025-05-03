using Helltal.Gelercat;
using NPBehave;
using UnityEngine;

public class MRBunnyController : GuestBase, IHurtable
{
    // Start is called before the first frame update
    [Header("MR-Bunny的移动速度")]
    public float moveSpeed = 3.5f; // 移动速度
    [Header("MR-Bunny的追击速度")]
    public float chaseSpeed = 5f; // 追击速度
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
    }
    private Node getDamageNode;
    protected override Root GetBehaviorTree()
    {
        getDamageNode = BuildGetDamageBranch(); // 受伤被打断
        return new Root(
            new Selector(
                BuildDeadBranch(),
                new Selector(
                    getDamageNode,
                    // BuildChesingBranch(), // 追击
                    BuildAttackBranch(),
                    BuildPatrolBranch() // 巡逻 
                )

            )


        );

    }
    private Node BuildPatrolBranch()
    {
        return new Condition(IsNavAgentOnNavmesh,
            new Sequence(     
                new Action(
                    () =>
                    {
                        agent.speed = moveSpeed; // 设置巡逻速度
                    }
                ),
             
                new Repeater(
                new Cooldown(1f,
                new Patrol(agent, navPointsManager))
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
                new WaitUntilStopped()

            ));
    }
    void OnHurtEnd()
    {
        Debug.Log("监听到：受伤结束！");
        behaviorTree.Blackboard["getDamage"] = false; // 重置受伤标志
    }
    private Node BuildAttackBranch()
    {
        return new Condition(() => { return TryAttack() || behaviorTree.Blackboard["isAttacking"].Equals(true); }, Stops.IMMEDIATE_RESTART,
            new Sequence(
                new Action(() =>
                {
                    behaviorTree.Blackboard["isAttacking"] = true; // 设置攻击标志
                    presenter.SetTrigger("Attack"); // 播放攻击动画
                }),
                new WaitUntilStopped()
            ));
    }
    // 监听攻击结束
    void OnAttackEnd()
    {
        Debug.Log("监听到：攻击结束！");
        behaviorTree.Blackboard["isAttacking"] = false; // 重置攻击标志
    }
    private bool TryAttack()
    {
        // debug
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("攻击！");
            return true;
        }

        return false;
    }
    // 实现IHurtable接口
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

}
