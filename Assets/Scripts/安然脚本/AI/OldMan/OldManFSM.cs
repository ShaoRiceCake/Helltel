using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldManFSM : FSM
{
    [Header("�˺�ֵ")] public float baseDamage = 5f;
    [Header("�ȴ�ʱ��")]public float waitingTimer = 180f;
    [Header("����ʱ��")] public float happyTimer = 3f;
    [Header("�������ɼ��ʱ��")] public float duration = 0.1f;
    [Header("��������뾶")] public float baseRadius = 0.1f;
    [Header("�������Ӱ뾶")] public float increaseRadius = 0.1f;
    public GameObject blackFog;
    public GameObject moneybag;

    protected override void Init()
    {
        base.Init();
        stateDic.Add(AIState.OldManWaiting, new OldManWaiting(this));
        stateDic.Add(AIState.OldManLonely, new OldManLonely(this));
        stateDic.Add(AIState.OldManHappy, new OldManHappy(this));
        stateDic.Add(AIState.OldManDie, new OldManDie(this));
        ChangeState(AIState.OldManWaiting);
    }

    public override void ChangeState(AIState next)
    {
        if (IsHost)
        {
            if(next==AIState.OldManHappy&& nameof(state) == AIState.OldManLonely.ToString())
            {
                var obj = Instantiate(moneybag, transform.position, Quaternion.identity);
                //var bag = obj.GetComponent<MoneyBag>();
                //bag.NetworkObject.Spawn();
                var lonely = state as OldManLonely;
                float a = 10f + lonely.waittimer * lonely.waittimer / 1000f + 0.3f * lonely.waittimer;
                //bag.SetMoneyServerRpc(a);
                Debug.Log("��Ǯ�� ");
            }
        }
        base.ChangeState(next);
    }
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
        waittime = manager.waitingTimer;
    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        waittime -= Time.deltaTime;
        if (waittime <= 0)
        {
            waittime = manager.waitingTimer;
            manager.ChangeState(AIState.OldManLonely);
            Debug.Log("���˽���¶�״̬");
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
            Debug.Log("���ɺ���");
            waittimer = 0f;
            manager.blackFog.transform.localScale += Vector3.one * count * manager.increaseRadius;
        }

        if (waittimer >= Random.Range(120f, 480f))
        {
            manager.ChangeState(AIState.OldManDie);
            Debug.Log("��������״̬");
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
            Debug.Log("����ʱ�����������ȴ�ʱ��");
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
        if (waittimer >= manager.duration / 3f)
        {
            Debug.Log("�������ɺ���");
            waittimer = 0f;
            manager.blackFog.transform.localScale += Vector3.one * lonely.count * manager.increaseRadius;
        }
    }
}

