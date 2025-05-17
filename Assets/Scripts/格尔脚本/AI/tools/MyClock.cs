using NPBehave;

using UnityEngine;



public class MyClock: Clock
{
    public void Tick()
    {
        base.Update(Time.deltaTime);  // 受 Time.timeScale 控制
    }
}