using UnityEngine;

public class PlayerControl_LeftFootControl : PlayerControl_FootControl
{
    protected override void Start()
    {
        base.Start();
        controlHandler.onLiftLeftLeg.AddListener(OnLiftLeftLeg);
        controlHandler.onReleaseLeftLeg.AddListener(OnReleaseLeftLeg);
    }

    protected override void UnsubscribeEvents()
    {
        controlHandler.onLiftLeftLeg.RemoveListener(OnLiftLeftLeg);
        controlHandler.onReleaseLeftLeg.RemoveListener(OnReleaseLeftLeg);
    }

    protected virtual void OnLiftLeftLeg() 
    {
        isFootUp = true;
        UnfixObject();
        springTool.isSpringEnabled = true;

    }

    protected virtual void OnReleaseLeftLeg()
    {
        isFootUp = false;
        springTool.isSpringEnabled = false;

    }

    protected override void Update()
    {
        base.Update();
    }

}
