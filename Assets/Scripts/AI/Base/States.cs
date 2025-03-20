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
    void Hurt();
}

public abstract class IState
{
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
