using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIState
{
    Patrol,Chasing,Attack,Die
}

public interface IHurt
{
    void Hurt();
}

public abstract class IState
{
    protected FSM manager;
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}

public class PatrolState : IState
{
    public int waypointIndex;
    public float waitTimer;
    public PatrolState(FSM manager)
    {
        this.manager = manager;
    }

    public override void Enter()
    {
        
    }

    public override void Exit()
    {
        
    }

    public override void Update()
    {
        PatrolCycle();
    }

    public void PatrolCycle()
    {
        if (manager.agent.remainingDistance < 0.2f)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer > 3f)//路径点等待大于3秒切换下一个路径点
            {
                if (waypointIndex < manager.path.waypoints.Count - 1)
                    waypointIndex++;
                else
                    waypointIndex = 0;
                manager.agent.speed = 3;
                manager.agent.SetDestination(manager.path.waypoints[waypointIndex].position);
                waitTimer = 0;
            }
        }
    }



    
}

public class ChasingState : IState
{
    public ChasingState(FSM manager)
    {
        this.manager = manager;
    }

    public override void Enter()
    {

    }

    public override void Exit()
    {

    }

    public override void Update()
    {

    }
}

public class AttackState : IState
{
    public AttackState(FSM manager)
    {
        this.manager = manager;
    }

    public override void Enter()
    {

    }

    public override void Exit()
    {

    }

    public override void Update()
    {

    }
}

public class DieState : IState
{
    public DieState(FSM manager)
    {
        this.manager = manager;
    }

    public override void Enter()
    {

    }

    public override void Exit()
    {

    }

    public override void Update()
    {

    }
}


