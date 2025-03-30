using UnityEngine;
using Obi;

public class ObiCatchTest : MonoBehaviour
{
    public ObiParticleAttachment attachment;
    private ObiActor _actor;
    public GameObject target1;
    public GameObject target2;
    private bool _isBluePrintLoaded;


    private void Start()
    {
        _actor = GetComponent<ObiActor>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            Snap(target1);
        }
        if (Input.GetKey(KeyCode.E))
        {
            Snap(target2);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            attachment.enabled = false;
            attachment.target = null;
        }
    }
    
    private void Snap(GameObject target)
    {
        attachment.enabled = true;

        if (!attachment || !_actor.isLoaded)
            return;

        attachment.BindToTarget(target.transform);
    }
}
