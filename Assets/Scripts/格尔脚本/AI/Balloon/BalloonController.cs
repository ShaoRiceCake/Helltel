using System.Collections;
using Helltal.Gelercat;
using NPBehave;
using NUnit.Framework.Constraints;
using UnityEngine;
using System.Linq;
using UnityEngine.VFX;
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
    public float approachTime = 4.5f; // 追踪
    public float cooldownTime = 20f; // 冷却时间
    [Header("爆炸力度")]
    public float explosionForce = 1000f; // 爆炸力度
    private float _approachTimer = 0f;
    private float _cooldownTimer = 0f;
    [Header("特效")]    
    public GameObject effectPrefab; // 预制体引用
    private bool _isApproaching = false;
    private bool _isCoolingDown = false;
    private bool _exploded = false;
    private Transform effectAnchor;

    protected override void Awake()
    {
        base.Awake();
        effectAnchor = transform.Find("特效点");
    }
    protected override void Start()
    {
        base.Start();
        BehaviorTree = GetBehaviorTree();
#if UNITY_EDITOR
        var debugger = gameObject.AddComponent<Debugger>();
        debugger.BehaviorTree = BehaviorTree;
#endif
        BehaviorTree.Blackboard["isDead"] = false;
        BehaviorTree.Blackboard["chaseTimer"] = 0f;
        BehaviorTree.Start();
    }

    protected override Root GetBehaviorTree()
    {
        return new Root(
            new Selector(
                BuildDeadBranch(),
                new Selector(
                    new Condition(IsNavAgentOnNavmesh, Stops.IMMEDIATE_RESTART,
                        new Selector(
                            new Condition(() => !_isCoolingDown, Stops.LOWER_PRIORITY,
                                new Selector(
                                    BuildApproachBranch(),
                                    BuildcantseeApproachBranch()
                                )
                            )
                            { Label = "notCoolingDown" },
                            BuildPatrolBranch()
                        )
                    )
                    { Label = "NavAgentOnNavmesh" }

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
                if (_exploded) return;
                _exploded = true; // 设置为已爆炸
                StartCoroutine(ExplodeCoroutine());
            }),

            new WaitUntilStopped()
            ));
    }

    private IEnumerator ExplodeCoroutine()
    {
        yield return new WaitForSeconds(0.75f);
        Explode();
    }

    bool audioLock = false;
    private Node BuildApproachBranch()
    {
        // 追逐状态
        return new Condition(IsEnemyCanSee, Stops.LOWER_PRIORITY,
            new Sequence(
                new Action(() =>
                {
                    _approachTimer = approachTime; // 刷新追逐时间
                    _isApproaching = true; // 设置追逐状态


                }),

                new Action(() =>
                {
                    if (!_curTarget || !IsNavAgentOnNavmesh()) return;
                    agent.speed = approachSpeed; // 设置追逐速度
                    agent.SetDestination(_curTarget.transform.position); // 追逐目标

                })
            )
        );
    }
    private Node BuildcantseeApproachBranch() //持续追逐目标
    {
        return new Condition(() => _isApproaching, Stops.LOWER_PRIORITY_IMMEDIATE_RESTART,
            new Sequence(
                new Action(() =>
                {
                    if (!_curTarget || !IsNavAgentOnNavmesh()) return;
                    agent.speed = approachSpeed; // 设置追逐速度
                    agent.SetDestination(_curTarget.transform.position); // 追逐目标

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
                        AudioManager.Instance.Play("气球巡逻", loop: true, owner: this);
                    }
                )
                { },
                new Patrol(agent, navPointsManager, detectRadius) { Label = "Patrol" }
            );
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("气球受伤：" + damage);
        if (_exploded) return;
        curHealth.Value -= damage;
        Debug.Log("气球当前血量：" + curHealth.Value);
        if (!(curHealth.Value <= 0)) return;
        BehaviorTree.Blackboard["isDead"] = true;
        IsDead = true;
        Debug.Log("气球死亡");
        Debug.Log("气球当前血量：" + curHealth.Value);
    }

    // private void Explode()
    // {
    //     //
    //     var hitColliders =
    //         Physics.OverlapSphere(transform.position, explosionRadius, LayerMask.NameToLayer("PlayerDetect"));

    //     var hit = hitColliders[0];
    //     if (hit != null)
    //     {
    //         var bodyball = hit.transform.root.Find("MoveBodyBall");
    //         var dist = Vector3.Distance(transform.position, hit.transform.position);
    //         var damage = Mathf.RoundToInt(Mathf.Lerp(explosionDamage, 0f, dist / explosionRadius));
    //         GameController.Instance.DeductHealth(damage);

    //         // add force on bodyball
    //         if (bodyball != null)
    //         {
    //             var rb = bodyball.GetComponent<Rigidbody>();
    //             if (rb != null)
    //             {
    //                 rb.AddExplosionForce(1000f, transform.position, explosionRadius);
    //                 Debug.Log("气球爆炸，给身体施加了力");
    //             }
    //         }
    //         Debug.Log("气球爆炸，没有施加力");
    //         AudioManager.Instance.Stop("气球靠近", owner: this);
    //         AudioManager.Instance.Play("气球爆炸", this.transform.position);

    //         Destroy(gameObject);
    //     }
    // }
    private void Explode()
    {
        int layerMask = 1 << LayerMask.NameToLayer("PlayerDetect");
        var hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, layerMask);

        foreach (var hit in hitColliders)
        {
            // 向上找到Player根节点（你也可以用tag或脚本识别）
            Transform playerRoot = hit.transform;
            while (playerRoot != null && playerRoot.name != "Player")
            {
                playerRoot = playerRoot.parent;
            }
            if (playerRoot == null) continue;


            // 递归查找MoveBodyBall
            var moveBodyBall = playerRoot.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.name == "MoveBodyBall");
            if (moveBodyBall != null)
            {
                var rb = moveBodyBall.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // 计算方向与爆炸力
                    Vector3 forceDir = (moveBodyBall.position - effectAnchor.position).normalized;
                    
                    rb.AddForce(forceDir * explosionForce, ForceMode.Impulse);
                }
                break; // 找到第一个就可以了
            }

            // 插值伤害（以Player根节点为距离参考）
            float dist = Vector3.Distance(transform.position, playerRoot.position);            
            int damage = Mathf.RoundToInt(Mathf.Lerp(explosionDamage, 0f, dist / explosionRadius));
            GameController.Instance.DeductHealth(damage);
        }

        // 音效与销毁
        AudioManager.Instance.Stop("气球靠近", owner: this);
        AudioManager.Instance.Play("气球爆炸", this.transform.position);

        var fx = Instantiate(effectPrefab, effectAnchor.position, Quaternion.identity);
        fx.transform.localScale = new Vector3(2, 2, 2);
        Destroy(fx, 1f);
        Destroy(this.gameObject,0.05f);
    }
    protected override void Update()
    {

        base.Update();
        if (_isApproaching)
        {
            _approachTimer -= Time.deltaTime;
            if (_approachTimer <= 0f)
            {
                _isApproaching = false; // 结束追逐状态
                _cooldownTimer = cooldownTime; // 开启冷却时间
                _isCoolingDown = true; // 设置冷却状态
            }
        }
        if (!_isCoolingDown) return;
        _cooldownTimer -= Time.deltaTime;
        if (_cooldownTimer <= 0f)
        {
            _isCoolingDown = false; // 结束冷却状态
        }
    }

}
