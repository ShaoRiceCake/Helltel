using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWeapon_BoxingGlove : PassiveItem
{
    private void Start()
    {
        OnGrabbed.AddListener(HandleWeaponGrabbed);
        OnReleased.AddListener(HandleWeaponReleased);
    }
 
    private void HandleWeaponGrabbed()
    {
       
    }
    private void HandleWeaponReleased()
    {
        
    }

    private void OnDestroy()
    {
        OnGrabbed.RemoveListener(HandleWeaponGrabbed);
        OnReleased.RemoveListener(HandleWeaponReleased);
    }
}
