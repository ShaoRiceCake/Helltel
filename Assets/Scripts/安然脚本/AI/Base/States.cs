using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIState
{
    OldManWaiting, OldManLonely, OldManHappy, OldManDie,
    MothCocoon, MothIncubate, MothChasing, MothAnger, MothPatrol

}

public interface IHurt
{
    void Hurt<T1,T2,T3>(Msg3T<T1,T2,T3> msg);
}


public class Msg3T<T1, T2,T3>
{
    public T1 t1;
    public T2 t2;
    public T3 t3;
    public Msg3T(T1 t1,T2 t2, T3 t3)
    {
        this.t1 = t1;
        this.t2 = t2;
        this.t3 = t3;
    }
}

public abstract class IState
{
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}


