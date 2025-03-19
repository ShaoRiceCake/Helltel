using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIState
{
    OldManWaiting, OldManLonely, OldManHappy, OldManDie

}

public enum AIType
{
    OldMan
}

public interface IHurt
{
    void Hurt();
}

public abstract class IState
{
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}

public class OldManWaiting : IState
{
    OldManFSM manager;
    float waittime = 0f;
    public OldManWaiting(FSM manager)
    {
        this.manager = manager as OldManFSM;
    }

    public override void Enter()
    {
        waittime = manager. waitingTimer;
    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        waittime -= Time.deltaTime;
        if (waittime <= 0 )
        {
            waittime =manager. waitingTimer;
            manager.ChangeState(AIState.OldManLonely);
            Debug.Log("老人进入孤独状态");
        }
    }
}

public class OldManLonely : IState
{
    OldManFSM manager;
    public float waittimer = 0f;
    public int count = 0;
    public OldManLonely(FSM manager)
    {
        this.manager = manager as OldManFSM;
    }

    public override void Enter()
    {
        waittimer = 0f;
        count = 0;
        manager.blackFog.transform.localScale = manager.baseRadius * Vector3.one;
    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        waittimer += Time.deltaTime;
        if (waittimer >= manager.duration)
        {
            count++;
            Debug.Log("生成黑雾");
            waittimer = 0f;
            manager.blackFog.transform.localScale +=Vector3.one * count * manager.increaseRadius;
        }

        if (waittimer >= Random.Range(120f, 480f))
        {
            manager.ChangeState(AIState.OldManDie);
            Debug.Log("进入死亡状态");
        }
    }
}

public class OldManHappy : IState
{
    OldManFSM manager;
    float waittime = 0f;
    public OldManHappy(FSM manager)
    {
        this.manager = manager as OldManFSM;
    }

    public override void Enter()
    {
        waittime = 0f;
    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        waittime += Time.deltaTime;
        if (waittime >= manager.happyTimer)
        {
            manager.ChangeState(AIState.OldManWaiting);
            Debug.Log("开心时间结束，进入等待时间");
        }
    }
}

public class OldManDie : IState
{
    OldManFSM manager;
    OldManLonely lonely;
    float waittimer = 0f;
    public OldManDie(FSM manager)
    {
        this.manager = manager as OldManFSM;
        lonely = manager.GetState(AIState.OldManLonely) as OldManLonely;
    }

    public override void Enter()
    {
        waittimer = 0f;

    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        waittimer += Time.deltaTime;
        if (waittimer >= manager.duration/3f)
        {
            Debug.Log("翻倍生成黑雾");
            waittimer = 0f;
            manager.blackFog.transform.localScale += Vector3.one * lonely.count * manager.increaseRadius;
        }
    }
}
