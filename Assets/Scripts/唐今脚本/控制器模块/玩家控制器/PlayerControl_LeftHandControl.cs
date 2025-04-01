using UnityEngine;

public class PlayerControl_LeftHandControl : PlayerControl_HandControl
{
    protected override void UnsubscribeEvents()
    {
        base.UnsubscribeEvents();
        controlHandler.onLiftLeftHand.RemoveListener(OnLiftLeftHand);
        controlHandler.onReleaseLeftHand.RemoveListener(OnReleaseLeftHand);
    }

    protected override void Update()
    {
        base.Update();

        if (CurrentPlayerHand == 1 && !IsHandActive)
        {
            ActivateControlBall();
        }
        else if (CurrentPlayerHand != 1 && IsHandActive)
        {
            DeactivateControlBall();
        }
    }
}