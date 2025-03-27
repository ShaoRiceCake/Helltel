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
        IsFootUp = true;
        UnfixObject();
        SpringTool.isSpringEnabled = true;

    }

    protected virtual void OnReleaseLeftLeg()
    {
        IsFootUp = false;
        SpringTool.isSpringEnabled = false;

    }

    protected override void Update()
    {
        base.Update();
    }

}
