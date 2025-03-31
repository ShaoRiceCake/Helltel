using UnityEngine;

public class PlayerControl_RightHandControl : PlayerControl_HandControl
{
    protected override void UnsubscribeEvents()
    {
        controlHandler.onLiftRightHand.RemoveListener(OnLiftRightHand);
        controlHandler.onReleaseRightHand.RemoveListener(OnReleaseRightHand);
    }

    protected override void Update()
    {
        base.Update();

        if (CurrentPlayerHand == 2 && !HandObject)
        {
            HandObject = Instantiate(HandBallPrefab, ObiGetGroupParticles.GetParticleWorldPositions(handControlAttachment)[0], Quaternion.identity);
            handControlAttachment.target = HandObject.transform;
            CatchTool.CatchBall = HandObject;
        }
        else
        {
            if (!HandObject || CurrentPlayerHand == 2) return;

            Destroy(HandObject);
            CatchTool.CatchBall = null;
            HandObject = null;
            handControlAttachment.target = null;
        }
    }
    
}
