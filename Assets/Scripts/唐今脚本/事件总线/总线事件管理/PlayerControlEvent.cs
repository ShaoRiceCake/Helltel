
// 控制模式变更事件
public class ControlModeChangedEvent
{
    public PlayerControlInformationProcess.ControlMode NewMode { get; }

    public ControlModeChangedEvent(PlayerControlInformationProcess.ControlMode newMode)
    {
        NewMode = newMode;
    }
}

// 控制权限锁定事件
public class ControlLockEvent
{
    public bool LockLegs { get; }
    public bool LockHands { get; }
    public bool LockAll => LockLegs && LockHands;

    public ControlLockEvent(bool lockLegs, bool lockHands)
    {
        LockLegs = lockLegs;
        LockHands = lockHands;
    }
}

// 全部解锁事件
public class UnlockAllControlsEvent { }