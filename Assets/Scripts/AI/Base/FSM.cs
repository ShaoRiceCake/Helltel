using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System;

public class FSM : NetworkBehaviour
{
    public AIType AItype;
    public Pathlist path;
    public NavMeshAgent agent;
    protected IState state;
    protected Dictionary<AIState, IState> stateDic = new Dictionary<AIState, IState>();
    private void Start()
    {
        if (IsHost)
        {
            agent = GetComponent<NavMeshAgent>();
            path = GameObject.Find("PathList").GetComponent<Pathlist>();
            SwitchType();
        }

    }

    void SwitchType()
    {
        switch (AItype)
        {
            case AIType.OldMan:
                stateDic.Add(AIState.OldManWaiting, new OldManWaiting(this));
                stateDic.Add(AIState.OldManLonely, new OldManLonely(this));
                stateDic.Add(AIState.OldManHappy, new OldManHappy(this));
                stateDic.Add(AIState.OldManDie, new OldManDie(this));
                ChangeState(AIState.OldManWaiting);break;
        }
    }

    private void Update()
    {
        if (IsHost)
        {
            if (state != null)
                state.Update();
        }
    }

    public virtual void ChangeState(AIState next)
    {
        if (IsHost)
        {
            if (state != null) state.Exit();
            state = stateDic[next];
            state.Enter();
        }
    }

    public virtual IState GetState(AIState state)
    {
        return stateDic[state];
    }

    public virtual IState GetCurrState()
    {
        return state;
    }
}
