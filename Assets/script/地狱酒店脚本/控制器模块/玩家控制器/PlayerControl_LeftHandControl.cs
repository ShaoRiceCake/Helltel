using UnityEngine;

public class PlayerControl_LeftHandControl : PlayerControl_HandControl
{
    protected override void UnsubscribeEvents()
    {
        ControlHandler.onLiftLeftHand.RemoveListener(OnLiftLeftHand);
        ControlHandler.onReleaseLeftHand.RemoveListener(OnReleaseLeftHand);
    }

    protected override void Update()
    {
        base.Update();

        if (CurrentPlayerHand == 1 && !HandObject)
        {
            HandObject = Instantiate(HandBallPrefab, ObiGetGroupParticles.GetParticleWorldPositions(handControlAttachment)[0], Quaternion.identity);
            handControlAttachment.target = HandObject.transform;
            catchTool.CatchBall = HandObject;
        }
        else
        {
            if (!HandObject || CurrentPlayerHand == 1) return;
            
            Destroy(HandObject);
            catchTool.CatchBall = null;
            HandObject = null; 
            handControlAttachment.target = null;

        }
    }
}
