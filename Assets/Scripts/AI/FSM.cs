using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
public class FSM : NetworkBehaviour
{
    public Pathlist path;
    public NavMeshAgent agent;
    private IState state;
    private Dictionary<AIState, IState> stateDic = new Dictionary<AIState, IState>();
    private void Start()
    {
        if (IsHost)
        {

            agent = GetComponent<NavMeshAgent>();
            path = GameObject.Find("PathList").GetComponent<Pathlist>();
            stateDic.Add(AIState.Patrol, new PatrolState(this));
            stateDic.Add(AIState.Attack, new AttackState(this));
            stateDic.Add(AIState.Chasing, new ChasingState(this));
            stateDic.Add(AIState.Die, new DieState(this));
            ChangeState(AIState.Patrol);
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

    public void ChangeState(AIState next)
    {
        if (IsHost)
        {
            if (state != null) state.Exit();
            state = stateDic[next];
            state.Enter();
        }
    }
}
