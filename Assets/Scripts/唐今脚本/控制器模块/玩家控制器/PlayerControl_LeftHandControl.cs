using UnityEngine;

public class PlayerControl_LeftHandControl : PlayerControl_HandControl
{

    protected override void UnsubscribeEvents()
    {
        controlHandler.onLiftLeftHand.RemoveListener(OnLiftLeftHand);
        controlHandler.onReleaseLeftHand.RemoveListener(OnReleaseLeftHand);
    }

    protected override void Update()
    {
        base.Update();

        if (CurrentPlayerHand == 1 && !HandObject)
        {
            HandObject = Instantiate(HandBallPrefab, ObiGetGroupParticles.GetParticleWorldPositions(handControlAttachment)[0], Quaternion.identity);
            handControlAttachment.target = HandObject.transform;
            CatchTool.CatchBall = HandObject;
        }
        else
        {
            if (!HandObject || CurrentPlayerHand == 1) return;
            
            Destroy(HandObject);
            CatchTool.CatchBall = null;
            HandObject = null; 
            handControlAttachment.target = null;

        }
    }
}
