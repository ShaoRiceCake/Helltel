using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System;

public class FSM : NetworkBehaviour
{
    [Header("最大生命值")] public float maxHealth = 15f;
    [Header("当前生命值")] public NetworkVariable<float> curHealth = new NetworkVariable<float>();
    [Header("熵值")]
    public float entropyValue;
    public GameObject[] players;
    public Pathlist path;
    public NavMeshAgent agent;
    protected IState state;
    protected Dictionary<AIState, IState> stateDic = new Dictionary<AIState, IState>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Init();
    }

    protected virtual void Init()
    {
        if (IsHost)
        {
            agent = GetComponent<NavMeshAgent>();
            path = GameObject.Find("PathList").GetComponent<Pathlist>();
            players = GameObject.FindGameObjectsWithTag("Player");
        }
    }

    public virtual void SetDestination(Transform target)
    {
        if (IsHost)
        {
            agent.SetDestination(target.position);
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
