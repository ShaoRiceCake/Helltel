using UnityEngine;

// 玩家位移事件类
public class PlayerTeleportEvent
{
    public Vector3 TargetPosition { get; private set; }
    
    public PlayerTeleportEvent(Vector3 targetPosition)
    {
        TargetPosition = targetPosition;
    }
}