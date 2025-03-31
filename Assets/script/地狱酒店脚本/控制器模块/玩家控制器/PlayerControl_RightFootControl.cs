public class PlayerControl_RightFootControl : PlayerControl_FootControl
{
    protected override void Start()
    {
        base.Start();
        ControlHandler.onLiftRightLeg.AddListener(OnLiftRightLeg);
        ControlHandler.onReleaseRightLeg.AddListener(OnReleaseRightLeg);
        ControlHandler.onLiftLeftLeg.AddListener(OnOtherFootLifted);
        ControlHandler.onReleaseLeftLeg.AddListener(OnOtherFootReleased);
    }

    protected override void UnsubscribeEvents()
    {
        ControlHandler.onLiftRightLeg.RemoveListener(OnLiftRightLeg);
        ControlHandler.onReleaseRightLeg.RemoveListener(OnReleaseRightLeg);
        ControlHandler.onLiftLeftLeg.RemoveListener(OnOtherFootLifted);
        ControlHandler.onReleaseLeftLeg.RemoveListener(OnOtherFootReleased);
    }

    private void OnLiftRightLeg() 
    {
        TryLiftFoot();
    }

    private void OnReleaseRightLeg() 
    {
        ReleaseFoot();
    }
    
    private void OnOtherFootLifted()
    {
        LockFoot();
    }
    
    private void OnOtherFootReleased()
    {
        UnlockFoot();
    }
}