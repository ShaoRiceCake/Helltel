using UnityEngine;
using Obi;

public class ObiCatchTest : MonoBehaviour
{
    public ObiParticleAttachment attachment;
    private ObiActor _actor;
    public GameObject target1;
    public GameObject target2;
    private bool _isBluePrintLoaded;
    
	
    void Start()
    {
        _actor = GetComponent<ObiActor>();
        _actor.OnBlueprintUnloaded += (_,_) => _isBluePrintLoaded = true;
    }

    private void Update()
    {
        if (!_isBluePrintLoaded) return;
        
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
        Debug.Log("enter");
        
        attachment.enabled = true;

        if (!attachment || !_actor.isLoaded)
            return;

        attachment.BindToTarget(target.transform);
    }
}
