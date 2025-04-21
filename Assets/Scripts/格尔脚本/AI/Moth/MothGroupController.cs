using NPBehave;
using Helltal.Gelercat;
using UnityEngine;
using System.Collections.Generic;
using Helltal;
public class MothGroupController : GuestBase
{
    private List<MothController> mothList = new List<MothController>(); //虫群列表

    [Header("集群行为参数")]
    /// <summary>
    ///  集群的影响范围
    /// </summary>
    public float nearDist = 30f; //判定为附近的虫子的最小范围值
    [Header("虫群的碰撞范围")]
    public float collisionDist = 5f; //判定为最近的虫子的最小范围值(具有碰撞风险)


    [Header("速度匹配系数")]
    public float velocityMatchingAmt = 0.01f; //与 附近的虫子的平均速度 乘数(影响新速度)
    [Header("虫群向心系数")]
    public float flockCenteringAmt = 0.15f; //与 附近的虫子的平均三维间距 乘数(影响新速度)
    [Header("虫群互斥系数")]
    public float collisionAvoidanceAmt = -0.5f; //与 最近的虫子的平均三维间距 乘数(影响新速度)
    [Header("插值系数，0偏向于当前速度，1偏向于目标速度")]

    public float velocityLerpAmt = 0.25f; //线性插值法计算新速度的 乘数
    [Header("目标吸引")]
    public float TargetAmt = 0.01f; // 目标吸引力系数(影响新速度)


    private Root behaviorTree;

    public GameObject CurTarget;
    
    public List<GameObject> EnemyList = new List<GameObject>(); //敌人列表

    protected override void Start()
    {
        base.Start();

        sensor = this.gameObject.GetComponent<GuestSensor>();
        if (sensor == null)
        {
            sensor = this.gameObject.AddComponent<GuestSensor>();

        }
        behaviorTree = GetBehaviorTree();

#if UNITY_EDITOR
        Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
        debugger.BehaviorTree = behaviorTree;
#endif
        behaviorTree.Start();
    }

    public void RegisterMoth(GameObject moth)
    {
        MothController mothController = moth.GetComponent<MothController>();
        if (mothController == null)
        {
            if (Debugging) Debug.LogError("MothController is null! Please check the moth prefab.");
            return;
        }
        if (!mothList.Contains(mothController))
        {
            mothList.Add(mothController);
            mothController.belongToGroup = this; //设置所属虫群
        }
    }
    public void UnregisterMoth(GameObject moth)
    {
        MothController mothController = moth.GetComponent<MothController>();
        if (mothController == null)
        {
            if (Debugging) Debug.LogError("MothController is null! Please check the moth prefab.");
            return;
        }
        if (mothList.Contains(mothController))
        {
            mothList.Remove(mothController);
            mothController.belongToGroup = null; //设置所属虫群为空
        }
    }

    // 巡逻
    // 激怒状态，跟随玩家
    protected override Root GetBehaviorTree()
    {
        return new Root(
            new Selector(
            
                BuildChaseingBranch(),//追击分支
                new Selector(
                    new Repeater(
                        new Cooldown(1f, 
                        new Patrol(agent, navPointsManager))
                    )
                )
            )
        );
    }
    // 
     private Node BuildChaseingBranch()
    {
        return new Condition(IsEnemyCanSee, Stops.IMMEDIATE_RESTART,
            new Action(() =>
            {
                if (CurTarget != null)
                {
                    agent.SetDestination(CurTarget.transform.position);
                    if (Debugging) Debug.Log("虫子正在追击目标：" + CurTarget.name);
                }
            })
        );
    }

    bool IsEnemyCanSee()
    {
        if(sensor != null)
        {
            if (sensor.detectedTargets.Count > 0)
            {
                foreach(var target in sensor.detectedTargets)
                {
                    if(EnemyList.Contains(target.gameObject))
                    {
                        CurTarget = target.gameObject;
                        return true;
                    }
                }
            }
        }
        return false;
    }
    
    public List<MothController> GetMothList()
    {
        return mothList;
    }
}
