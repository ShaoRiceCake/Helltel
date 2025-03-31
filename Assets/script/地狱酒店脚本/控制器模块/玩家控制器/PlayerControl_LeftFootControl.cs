public class PlayerControl_LeftFootControl : PlayerControl_FootControl
{
    protected override void Start()
    {
        base.Start();
        controlHandler.onLiftLeftLeg.AddListener(OnLiftLeftLeg);
        controlHandler.onReleaseLeftLeg.AddListener(OnReleaseLeftLeg);
        controlHandler.onLiftRightLeg.AddListener(OnOtherFootLifted);
        controlHandler.onReleaseRightLeg.AddListener(OnOtherFootReleased);
    }

    protected override void UnsubscribeEvents()
    {
        controlHandler.onLiftLeftLeg.RemoveListener(OnLiftLeftLeg);
        controlHandler.onReleaseLeftLeg.RemoveListener(OnReleaseLeftLeg);
        controlHandler.onLiftRightLeg.RemoveListener(OnOtherFootLifted);
        controlHandler.onReleaseRightLeg.RemoveListener(OnOtherFootReleased);
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