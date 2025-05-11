using System.Collections;
using Helltal.Gelercat;
using NPBehave;
using NUnit.Framework.Constraints;
using UnityEngine;

public class BalloonController : GuestBase, IHurtable
{
    [Header("气球移动参数")]
    public float patrolSpeed = 1f;
    public float approachSpeed = 1f;
    [Header("搜索精度")]
    public float detectRadius = 5f;
    public float coolDownTime = 20f;

    [Header("爆炸参数")]
    public float explosionRadius = 5f;
    public float explosionDamage = 80f;
    [Header("时间参数")]
    public float approachTime = 5f; // 追踪
    public float cooldownTime = 20f; // 冷却时间
    private GameObject curTarget;
    private float approachTimer = 0f;
    private float cooldownTimer = 0f;

    private bool isApproaching = false;
    private bool isCoolingdown = false;
    private bool exploded = false;

    protected override void Awake()
    {
        base.Awake();

        sensor = this.gameObject.GetComponent<GuestSensor>();
        sensor.viewDistance = detectRadius; // 气球的搜索精度和探测范围是一致的
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
        Debugger debugger = gameObject.AddComponent<Debugger>();
        debugger.BehaviorTree = behaviorTree;
#endif
        behaviorTree.Blackboard["isDead"] = false;
        behaviorTree.Blackboard["chaseTimer"] = 0f;
        behaviorTree.Start();
    }

    protected override Root GetBehaviorTree()
    {
        return new Root(
            new Selector(
                BuildDeadBranch(),
                new Selector(
                    new Condition(IsNavAgentOnNavmesh, Stops.IMMEDIATE_RESTART,
                        new Selector(
                            new Condition(() => !isCoolingdown, Stops.IMMEDIATE_RESTART,
                                new Selector(
                                    BuildApproachBranch(),
                                    BuildcantseeApproachBranch()
                                )
                            ),

                            BuildPatrolBranch()
                        )
                    )

                )
            )
        );
    }

    private Node BuildDeadBranch()
    {
        return new BlackboardCondition("isDead", Operator.IS_EQUAL, true, Stops.SELF,
            new Sequence(
            new Action(() =>
            {
                agent.isStopped = true; // 停止移动
                agent.ResetPath(); // 重置路径

            }),
            new Action(() =>
            {
                if (!exploded)
                {
                    exploded = true; // 设置为已爆炸
                    StartCoroutine(ExplodeCoroutine());
                }
            }),

            new WaitUntilStopped()
            ));
    }
    IEnumerator ExplodeCoroutine()
    {
        // 等待一段时间后执行爆炸
        yield return new WaitForSeconds(0.5f);
        Explode();
    }
    private Node BuildApproachBranch()
    {
        // 追逐状态
        return new Condition(() => IsEnemyCanSee(), Stops.LOWER_PRIORITY,
            new Sequence(
                new Action(() =>
                {
                    approachTimer = approachTime; // 刷新追逐时间
                    isApproaching = true; // 设置追逐状态
                }),
                new Action(() =>
                {
                    if (curTarget != null && IsNavAgentOnNavmesh())
                    {
                        agent.speed = approachSpeed; // 设置追逐速度
                        agent.SetDestination(curTarget.transform.position); // 追逐目标
                    }
                })
            )
        );
    }
    private Node BuildcantseeApproachBranch() //持续追逐目标
    {
        return new Condition(() => isApproaching, Stops.LOWER_PRIORITY_IMMEDIATE_RESTART,
            new Sequence(
                new Action(() =>
                {
                    if (curTarget != null && IsNavAgentOnNavmesh())
                    {
                        agent.speed = approachSpeed; // 设置追逐速度
                        agent.SetDestination(curTarget.transform.position); // 追逐目标
                    }
                })
            )
        );
    }

    private Node BuildPatrolBranch()
    {
        return new Sequence(
                new Action(
                    () =>
                    {
                        agent.speed = patrolSpeed; // 设置巡逻速度
                    }
                ),

                new Repeater(
                new Cooldown(1f,
                new Patrol(agent, navPointsManager, detectRadius))
            )
            );
    }

    public void TakeDamage(int damage)
    {
        if (!exploded)
        {
            curHealth.Value -= damage;
            if (curHealth.Value <= 0)
            {
                behaviorTree.Blackboard["isDead"] = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!exploded && GameManager.instance.playerIdentifiers.Contains(other.transform))
        {
            behaviorTree.Blackboard["isDead"] = true;
        }
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
                    if (GameManager.instance.playerIdentifiers.Contains(target.gameObject.transform)) // 单机版本特供，仅有一个玩家
                    {
                        curTarget = target.gameObject;
                        return true;
                    }
                }
            }
        }
        return false;
    }
    private void Explode()
    {
        //
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius,LayerMask.NameToLayer("PlayerDetect"));

        ///<summary>
        /// 多玩家时
        ///<summary>
        foreach (var hit in hitColliders)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            int damage = Mathf.RoundToInt(Mathf.Lerp(explosionDamage, 0f, dist / explosionRadius));
            ///<summary>
            /// 多玩家时
            ///<summary>
            // 玩家s受伤 IHurtable 接口
            // hit.GetComponent<IHurtable>()?.TakeDamage(Mathf.RoundToInt(damage));
            ///<summary>
            /// 单玩家特供
            ///<summary>
            GameController.Instance.DeductHealth(damage);
        }

 
        // 播放爆炸动画或特效
        Debug.Log("气球爆炸！");

        // 自毁
        Destroy(gameObject);
    }
    protected override void Update()
    {

        base.Update();
        if (isApproaching)
        {
            approachTimer -= Time.deltaTime;
            Debug.Log("气球追逐中，剩余时间：" + approachTimer);
            if (approachTimer <= 0f)
            {
                isApproaching = false; // 结束追逐状态
                cooldownTimer = cooldownTime; // 开启冷却时间
                isCoolingdown = true; // 设置冷却状态
            }
        }

        if (isCoolingdown)
        {
            cooldownTimer -= Time.deltaTime;
            Debug.Log("气球冷却中，剩余时间：" + cooldownTimer);
            if (cooldownTimer <= 0f)
            {
                isCoolingdown = false; // 结束冷却状态
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    
}
