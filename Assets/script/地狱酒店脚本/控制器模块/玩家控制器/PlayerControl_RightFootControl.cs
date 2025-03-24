public class PlayerControl_RightFootControl : PlayerControl_FootControl
{
    protected override void Start()
    {
        base.Start();
        controlHandler.onLiftRightLeg.AddListener(OnLiftRightLeg);
        controlHandler.onReleaseRightLeg.AddListener(OnReleaseRightLeg);
    }

    protected override void UnsubscribeEvents()
    {
        controlHandler.onLiftRightLeg.RemoveListener(OnLiftRightLeg);
        controlHandler.onReleaseRightLeg.RemoveListener(OnReleaseRightLeg);
    }

    protected virtual void OnLiftRightLeg() 
    {
        isFootUp = true;
        UnfixObject();
        springTool.isSpringEnabled = true;

    }

    protected virtual void OnReleaseRightLeg() 
    {
        isFootUp = false;
        springTool.isSpringEnabled = false;

    }
}
