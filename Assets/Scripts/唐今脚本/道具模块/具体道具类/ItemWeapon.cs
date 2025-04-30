using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemWeapon : PassiveItem
{

    private void Start()
    {
        OnGrabbed.AddListener(HandleWeaponGrabbed);
        OnReleased.AddListener(HandleWeaponReleased);

    }

    private void HandleWeaponGrabbed()
    {
        enableOrbit = true;
    }
    
    private void HandleWeaponReleased()
    {
        enableOrbit = false;
    }

    private void OnDestroy()
    {
        OnGrabbed.RemoveListener(HandleWeaponGrabbed);
        OnReleased.RemoveListener(HandleWeaponReleased);
    }
}
