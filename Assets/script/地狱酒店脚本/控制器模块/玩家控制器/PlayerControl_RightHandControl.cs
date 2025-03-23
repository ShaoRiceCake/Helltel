
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

        if (currentHand == 2 && !handObject)
        {
            handObject = Instantiate(handBallPrefab, ObiGetGroupParticles.GetParticleWorldPositions(handControlAttachment)[0], Quaternion.identity);

            handControlAttachment.target = handObject.transform;
        }
        else
        {
            if (handObject != null && currentHand != 2)
            {
                Destroy(handObject);
                handObject = null;
                handControlAttachment.target = null;
            }
        }
    }


}
