using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMoth : PassiveItem
{
    private MothController _mothController;
    
    
    protected override void Awake()
    {
        base.Awake();

        _mothController = GetComponent<MothController>();

        if (_mothController == null)
        {
            Debug.LogError("No MothController");
        }
        
        OnGrabbed.AddListener(Grabbed);
        OnReleased.AddListener(Released);
    }

    private void Grabbed()
    {
        _mothController.Grabb_HandleGrabb();
    }
    private void Released()
    {
        _mothController.Grabb_HandleRelease();
        Debug.Log("Released");
    }

}
