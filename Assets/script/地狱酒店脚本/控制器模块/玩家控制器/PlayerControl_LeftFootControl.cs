public class PlayerControl_LeftFootControl : PlayerControl_FootControl
{
    protected override void Start()
    {
        base.Start();
        ControlHandler.onLiftLeftLeg.AddListener(OnLiftLeftLeg);
        ControlHandler.onReleaseLeftLeg.AddListener(OnReleaseLeftLeg);
        ControlHandler.onLiftRightLeg.AddListener(OnOtherFootLifted);
        ControlHandler.onReleaseRightLeg.AddListener(OnOtherFootReleased);
    }

    protected override void UnsubscribeEvents()
    {
        ControlHandler.onLiftLeftLeg.RemoveListener(OnLiftLeftLeg);
        ControlHandler.onReleaseLeftLeg.RemoveListener(OnReleaseLeftLeg);
        ControlHandler.onLiftRightLeg.RemoveListener(OnOtherFootLifted);
        ControlHandler.onReleaseRightLeg.RemoveListener(OnOtherFootReleased);
    }

    private void OnLiftLeftLeg() 
    {
        TryLiftFoot();
    }

    private void OnReleaseLeftLeg()
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