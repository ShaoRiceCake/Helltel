using UnityEngine;

public class PlayerControl_RightHandControl : PlayerControl_HandControl
{
    protected override void UnsubscribeEvents()
    {
        base.UnsubscribeEvents();
        controlHandler.onLiftRightHand.RemoveListener(OnLiftRightHand);
        controlHandler.onReleaseRightHand.RemoveListener(OnReleaseRightHand);
    }

    protected override void Update()
    {
        base.Update();

        if (CurrentPlayerHand == 2 && !IsHandActive)
        {
            ActivateControlBall();
        }
        else if (CurrentPlayerHand != 2 && IsHandActive)
        {
            DeactivateControlBall();
        }
    }
}   