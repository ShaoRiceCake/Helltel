using NPBehave;
using Helltal.Gelercat;
using UnityEngine;
using System.Collections.Generic;
using Helltal;
using UnityEngine.Serialization;

public class MothGroupController : GuestBase
{
    private readonly List<MothController> _mothList = new List<MothController>(); //虫群列表

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
    public float targetAmt = 0.01f; // 目标吸引力系数(影响新速度)

    // public List<GameObject> EnemyList = new List<GameObject>(); //敌人列表
    public GameObject attachingMoth; //当前附着的虫子
    public object CurTarget { get; set; }



    protected override void Start()
    {
        base.Start();
        
        BehaviorTree = GetBehaviorTree();

#if UNITY_EDITOR
        var debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
        debugger.BehaviorTree = BehaviorTree;
#endif
        BehaviorTree.Start();
    }

    public void RegisterMoth(GameObject moth)
    {
        var mothController = moth.GetComponent<MothController>();
        if (!mothController)
        {
            if (debugging) Debug.LogError("MothController is null! Please check the moth prefab.");
            return;
        }

        if (_mothList.Contains(mothController)) return;
        _mothList.Add(mothController);
        mothController.belongToGroup = this; //设置所属虫群
    }
    public void UnregisterMoth(GameObject moth)
    {
        var mothController = moth.GetComponent<MothController>();
        if (!mothController)
        {
            if (debugging) Debug.LogError("MothController is null! Please check the moth prefab.");
            return;
        }

        if (!_mothList.Contains(mothController)) return;
        _mothList.Remove(mothController);
        mothController.belongToGroup = null; //设置所属虫群为空
    }

    protected override Root GetBehaviorTree()
    {
        return new Root(
            new Selector(

                BuildChasingBranch(),//追击分支
                new Selector(
                    new Condition(IsNavAgentOnNavmesh,
                        new Repeater(
                            new Cooldown(1f,
                            new Patrol(agent, navPointsManager))
                        ))
                    )
            )
        );
    }

    private Node BuildChasingBranch()
    {
        return new Condition(IsEnemyCanSee, Stops.IMMEDIATE_RESTART,
            new Action(() =>
            {
                if (_curTarget && agent.isOnNavMesh)
                {
                    agent.SetDestination(_curTarget.transform.position);
                }
            })
        );
    }

    public List<MothController> GetMothList()
    {
        return _mothList;
    }

}
