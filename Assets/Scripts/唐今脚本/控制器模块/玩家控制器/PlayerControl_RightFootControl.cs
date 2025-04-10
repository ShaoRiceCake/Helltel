public class PlayerControl_RightFootControl : PlayerControl_FootControl
{
    protected override void Start()
    {
        base.Start();
        controlHandler.onLiftRightLeg.AddListener(OnLiftRightLeg);
        controlHandler.onReleaseRightLeg.AddListener(OnReleaseRightLeg);
        controlHandler.onLiftLeftLeg.AddListener(OnOtherFootLifted);
        controlHandler.onReleaseLeftLeg.AddListener(OnOtherFootReleased);
        controlHandler.onCameraControl.AddListener(OnCameraControl);

    }

    protected override void UnsubscribeEvents()
    {
        controlHandler.onLiftRightLeg.RemoveListener(OnLiftRightLeg);
        controlHandler.onReleaseRightLeg.RemoveListener(OnReleaseRightLeg);
        controlHandler.onLiftLeftLeg.RemoveListener(OnOtherFootLifted);
        controlHandler.onReleaseLeftLeg.RemoveListener(OnOtherFootReleased);
        controlHandler.onCameraControl.RemoveListener(OnCameraControl);

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
    private void OnCameraControl()
    {
        if (CurrentState == FootState.Lifted)
        {
            ReleaseFoot();
        }
    }
}