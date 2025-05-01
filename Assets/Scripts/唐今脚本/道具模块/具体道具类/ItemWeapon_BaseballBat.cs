using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemWeapon_BaseballBat : PassiveItem
{
    [SerializeField] private GameObject associatedObject; // Reference to the external object



    private void FixedUpdate()
    {
         UpdateAssociatedObjectLayer();
    }

    private void UpdateAssociatedObjectLayer()
    {
        if (associatedObject)
        {
            associatedObject.layer = gameObject.layer;
        }
    }

}