
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
        if (currentHand == 1 && !handObject)
        {
            handObject = Instantiate(handBallPrefab, ObiGetGroupParticles.GetParticleWorldPositions(handControlAttachment)[0], Quaternion.identity);

            handControlAttachment.target = handObject.transform;
        }
        else
        {
            if (handObject != null)
            {
                Destroy(handObject);

                handControlAttachment.target = null;
            }
        }
    }
}
