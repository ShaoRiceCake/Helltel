using Helltal.Gelercat;
using NPBehave;
using UnityEngine;

public class BalloonController : GuestBase, IHurtable
{
    [Header("气球移动参数")]
    public float patrolSpeed = 1f;
    public float approachSpeed = 1f;
    public float detectRadius = 5f;
    public float approachDuration = 5f;
    public float coolDownTime = 20f;

    [Header("爆炸参数")]
    public float explosionRadius = 5f;
    public float explosionDamage = 80f;

    private GameObject curTarget;
    private float approachTimer = 0f;
    private float cooldownTimer = 0f;

    private bool isApproaching = false;
    private bool isCooldown = false;
    private bool exploded = false;

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
        Debugger debugger = gameObject.AddComponent<Debugger>();
        debugger.BehaviorTree = behaviorTree;
#endif
        behaviorTree.Blackboard["isDead"] = false;
        behaviorTree.Start();
    }

    protected override Root GetBehaviorTree()
    {
        return new Root(
            new Selector(
                BuildDeadBranch(),
                new Selector(
                    BuildApproachBranch(),
                    BuildPatrolBranch()
                )
            )
        );
    }

    private Node BuildDeadBranch()
    {
        return new BlackboardCondition("isDead", Operator.IS_EQUAL, true, Stops.SELF,
            new Action(() =>
            {
                if (!exploded)
                {
                    exploded = true;
                    Explode();
                }
            }));
    }

    private Node BuildApproachBranch()
    {
        return new Condition(() => isApproaching && !isCooldown && curTarget != null, Stops.NONE,
            new Action(() =>
            {
                if (agent.isOnNavMesh)
                {
                    float distance = Vector3.Distance(transform.position, curTarget.transform.position);
                    if (distance < 1f) // 玩家触碰
                    {
                        behaviorTree.Blackboard["isDead"] = true;
                        return;
                    }

                    agent.SetDestination(curTarget.transform.position);
                    agent.speed = approachSpeed;

                    approachTimer -= Time.deltaTime;
                    if (approachTimer <= 0f)
                    {
                        isApproaching = false;
                        isCooldown = true;
                        cooldownTimer = coolDownTime;
                    }
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
                        agent.speed = patrolSpeed; // 设置巡逻速度
                    }
                ),

                new Repeater(
                new Cooldown(1f,
                new Patrol(agent, navPointsManager))
            )
            ));
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

    private void Explode()
    {
        // 简单爆炸实现，可扩展为AOE + 伤害衰减
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hitColliders)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            float damage = Mathf.Lerp(explosionDamage, 0f, dist / explosionRadius);
            // 假设玩家带有 IHurtable 接口
            hit.GetComponent<IHurtable>()?.TakeDamage(Mathf.RoundToInt(damage));
        }

        // 播放爆炸动画或特效
        Debug.Log("气球爆炸！");

        // 自毁
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
