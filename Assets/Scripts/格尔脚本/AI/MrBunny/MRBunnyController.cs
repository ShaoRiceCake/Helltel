using System.Linq;
using Helltal.Gelercat;
using NPBehave;
using UnityEngine;
using UnityEngine.Serialization;

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
    
    private Node _getDamageNode;
    private Node _attackNode;
    private bool _canAttack = true;

    
    protected override void Start()
    {
        base.Start();
        BehaviorTree = GetBehaviorTree();
#if UNITY_EDITOR
        var debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
        debugger.BehaviorTree = BehaviorTree;
#endif
        BehaviorTree.Blackboard["getDamage"] = false; // 受伤标志
        BehaviorTree.Blackboard["isDead"] = false; // 死亡标志
        BehaviorTree.Blackboard["isAttacking"] = false; // 攻击标志
        BehaviorTree.Start();
    }

    protected override Root GetBehaviorTree()
    {
        _getDamageNode = BuildGetDamageBranch(); // 受伤被打断
        _attackNode = BuildAttackBranch(); // 攻击被打断
        return new Root(
            new Selector(
                BuildDeadBranch(),
                new Selector(
                    _getDamageNode,
                    _attackNode, // 攻击
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
                if (!_curTarget || !agent.isOnNavMesh) return;
                var distance = Vector3.Distance(transform.position, _curTarget.transform.position);
                if (distance <= attackDistance)
                {
                    agent.ResetPath(); // 停止导航
                    agent.speed = 0f;
                }
                else
                {
                    agent.SetDestination(_curTarget.transform.position);
                    agent.speed = (distance >= chaseDistance) ? moveSpeed : chaseSpeed;
                }
            }));
    }


    
    private Node BuildPatrolBranch()
    {
        return new Condition(IsNavAgentOnNavmesh, Stops.NONE,
            new Sequence(
                new Action(
                    () =>
                    {
                        agent.speed = moveSpeed;
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
                        if (!IsNavAgentOnNavmesh()) return;
                        agent.ResetPath(); // 停止追击
                        agent.velocity = Vector3.zero; // 停止移动
                        agent.speed = 0f; // 保证站定
                    }
                ),
                new WaitUntilStopped()

            ));
    }

    private void OnHurtEnd()
    {
        Debug.Log("监听到：受伤结束！");
        BehaviorTree.Blackboard["getDamage"] = false; // 重置受伤标志
    }
    
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
    private void OnAttackEnd()
    {
        Debug.Log("监听到：攻击结束！");
        BehaviorTree.Blackboard["isAttacking"] = false; // 重置攻击标志
        _canAttack = true; // 解除攻击锁

        if (_attackNode is not { CurrentState: Node.State.ACTIVE }) return;
        _attackNode.Stop();  // 强制退出攻击节点
        Debug.Log("强制退出攻击节点！");
    }
    
   public void TakeDamage(int damage)
    {
        if (damage >= curHealth.Value)
        {
            curHealth.Value = 0;
            Debug.Log("MR-Bunny死亡！当前血量：" + curHealth.Value);
            BehaviorTree.Blackboard["isDead"] = true; // 设置死亡标志
        }
        else
        {
            curHealth.Value -= damage;
            Debug.Log("MR-Bunny受伤！当前血量：" + curHealth.Value);
            if (_getDamageNode is { CurrentState: Node.State.ACTIVE })
            {
                _getDamageNode.Stop();
            }
            BehaviorTree.Blackboard["getDamage"] = true;
        }

    }

    protected override void Update()
    {
        if (_curTarget)
        {
            var distance = Vector3.Distance(transform.position, _curTarget.transform.position);
            if (distance <= attackDistance)
            {

                if (_canAttack && !BehaviorTree.Blackboard["isAttacking"].Equals(true))
                {
                    _canAttack = false;
                    BehaviorTree.Blackboard["isAttacking"] = true; // 手动触发攻击行为
                }
                agent.ResetPath();
                agent.speed = 0f;

            }
        }
        if (BehaviorTree.Blackboard["isAttacking"].Equals(true) && BehaviorTree.Blackboard["isDead"].Equals(false))
        {
            // 计算攻击目标的方向
            var targetPosition = _curTarget.transform.position;
            var direction = targetPosition - transform.position;
            direction.y = 0; // 确保只在水平方向旋转
            if (direction == Vector3.zero) return;
            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
        else if (_curTarget && BehaviorTree.Blackboard["isDead"].Equals(false))
        {
            agent.ResetPath(); // 停止追击
            agent.speed = 0f; // 保证站定

            var targetPosition = _curTarget.transform.position;
            var direction = targetPosition - transform.position;
            direction.y = 0; // 确保只在水平方向旋转
            if (direction == Vector3.zero) return;
            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
    protected override void LateUpdate()
    {
        base.LateUpdate();
        var horizontalVelocity = agent.velocity;
        horizontalVelocity.y = 0f; // 只考虑水平速度
        var currentSpeed = horizontalVelocity.magnitude;
        var normalizedSpeed = 0f;
        if (currentSpeed > 0.01f)
        {
            if (currentSpeed < moveSpeed)
            {
                normalizedSpeed = Mathf.InverseLerp(0, moveSpeed, currentSpeed) * 0.5f;
            }
            else
            {
                normalizedSpeed = Mathf.InverseLerp(moveSpeed, chaseSpeed, currentSpeed) * 0.5f + 0.5f;
            }
        }
        presenter.SetFloat("Speed", normalizedSpeed);
    }
}
