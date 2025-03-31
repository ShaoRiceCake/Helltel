using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class MothFSM : FSM,IHurt
{
    [Header("当前目标玩家")] public List<GameObject> targets = new List<GameObject>();
    [Header("伤害")] public float damage = 15f;

    [Header("进入孵化的碰撞距离")] public float distance;
    [Header("是否为领头蛾")] public bool IsLeader;
    [Header("羽衣蛾组")] public List<MothFSM> moths=new List<MothFSM>();
    public Animator animator;
    protected override void Init()
    {
        if (IsHost)
        {
            curHealth.Value = maxHealth;
            stateDic.Add(AIState.MothCocoon, new MothCocoon(this));
            stateDic.Add(AIState.MothIncubate, new MothIncubate(this));
            stateDic.Add(AIState.MothChasing, new MothChasing(this));
            stateDic.Add(AIState.MothAnger, new MothAnger(this));
            stateDic.Add(AIState.MothPatrol, new MothPatrol(this));
            ChangeState(AIState.MothCocoon);
            var allMoth = GameObject.FindObjectsOfType<MothFSM>();
            foreach(var m in allMoth)
            {
                m.moths.Add(this);
            }
        }
        base.Init();
    }

    public virtual MothFSM GetLeader()//孵化领头蛾
    {
        MothFSM leader = null;
        float shorter = Mathf.Infinity;
        foreach (var moth in moths)
        {
            MothIncubate m = moth.GetState(AIState.MothIncubate) as MothIncubate;
            if (m.randomTimer < shorter)
            {
                shorter = m.randomTimer;
                leader = moth;
            }
        }
        return leader;
    }

    public virtual GameObject GetNearTarget()//玩家内距离领头蛾最近的
    {
        GameObject target = null;
        float shorter = Mathf.Infinity;
        foreach (var t in targets)
        {
            if (Vector3.SqrMagnitude(t.transform.position-transform.position)<shorter)
            {
                shorter = Vector3.SqrMagnitude(t.transform.position - transform.position);
                target = t;
            }
        }
        return target;
    }

    public virtual WayPoint GetNearPatrolPoint()
    {
        WayPoint point = null;
        float shorter = Mathf.Infinity;
        foreach (var t in path.waypoints)
        {
            if (!t.isCheck)
            {
                if (Vector3.SqrMagnitude(t.point.position - transform.position) < shorter)
                {
                    shorter = Vector3.SqrMagnitude(t.point.position - transform.position);
                    point = t;
                }
            }

        }
        return point;
    }

    public void Hurt<T1, T2, T3>(Msg3T<T1, T2, T3> msg)
    {
        Debug.Log(msg.t1 + "===" + msg.t2 + "===" + msg.t3);
    }
}
/// <summary>
/// 茧
/// </summary>
public class MothCocoon : IState
{
    MothFSM manager;
    public MothCocoon(FSM manager)
    {
        this.manager = manager as MothFSM;
    }

    public override void Enter()
    {
       
    }

    public override void Exit()
    {
        
    }

    public override void Update()
    {
        foreach(GameObject p in manager.players)
        {
            if(Vector3.SqrMagnitude(p.transform.position - manager.transform.position)<= manager.distance* manager.distance)
            {
                foreach(var moth in manager.moths)
                {
                    moth.ChangeState(AIState.MothIncubate);
                }
                manager.targets.Add(p);
            }
        }
    }
}
/// <summary>
/// 孵化
/// </summary>
public class MothIncubate : IState
{
    MothFSM manager;
    public float randomTimer;
    float randomtime = 0f;
    public MothIncubate(FSM manager)
    {
        this.manager = manager as MothFSM;
    }

    public override void Enter()
    {
        randomtime = 0f;
        randomTimer = Random.Range(0f, 0.8f);
    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        randomtime += Time.deltaTime;
        if (randomtime >= randomTimer)
        {
            manager.GetLeader().IsLeader = true;
            manager.ChangeState(AIState.MothChasing);
        }

    }
}
/// <summary>
/// 自动寻路
/// </summary>
public class MothChasing : IState
{
    MothFSM manager;
    float chasingTimer = 10f;
    float chasingtime = 0f;
    public MothChasing(FSM manager)
    {
        this.manager = manager as MothFSM;
    }

    public override void Enter()
    {

    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        if (manager.IsLeader)
        {
            manager.SetDestination(manager.GetNearTarget().transform);
            chasingtime += Time.deltaTime;
            if (chasingtime >= chasingTimer)
            {
                manager.ChangeState(AIState.MothPatrol);
            }

            foreach(GameObject p in manager.targets)
            {
                if (Vector3.SqrMagnitude(p.transform.position - manager.transform.position) < 64f)
                {
                    manager.ChangeState(AIState.MothAnger);
                }
            }

        }
        else
        {
            manager.SetDestination(manager.GetLeader().transform);//不是领头蛾跟随领头蛾

        }

    }
}
/// <summary>
/// 激怒
/// </summary>
public class MothAnger : IState
{
    MothFSM manager;
    float angerTimer = 5f;
    float angertime = 0f;
    float attackTimer = 1f;
    float attacktime = 0f;
    public MothAnger(FSM manager)
    {
        this.manager = manager as MothFSM;
    }

    public override void Enter()
    {
        angertime = 0f;
        attacktime = 0f;
    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        attacktime += Time.deltaTime;
        if (attacktime >= attackTimer)
        {
            attacktime = 0f;
            manager.animator.Play("Attack", 0, 0);
        }

        angertime += Time.deltaTime;
        if (angertime >= angerTimer && manager.GetNearTarget() == null)
        {
            foreach (var moth in manager.moths)
            {
                moth.ChangeState(AIState.MothChasing);
            }
        }
    }
}
/// <summary>
/// 漫游
/// </summary>
public class MothPatrol : IState
{
    MothFSM manager;
    float waitTimer = 3f;
    float waitime = 0;
    int waypointIndex = 0;
    int waypointlenth;
    public MothPatrol(FSM manager)
    {
        this.manager = manager as MothFSM;
    }

    public override void Enter()
    {
        if (manager.IsLeader)
        {
            manager.SetDestination(manager.GetNearPatrolPoint().point);
        }
    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        if (manager.IsLeader)
        {
            manager.SetDestination(manager.GetNearPatrolPoint().point);

            if (Vector3.SqrMagnitude(manager.transform.position - manager.GetNearPatrolPoint().point.position) < 64f)
            {
                manager.GetNearPatrolPoint().Check(true);
                manager.path.ResetAllCheck();
            }

            foreach (GameObject p in manager.targets)
            {
                if (Vector3.SqrMagnitude(p.transform.position - manager.transform.position) < 64f)
                {
                    manager.ChangeState(AIState.MothAnger);
                }
            }

        }
        else
        {
            manager.SetDestination(manager.GetLeader().transform);
        }
    }
}