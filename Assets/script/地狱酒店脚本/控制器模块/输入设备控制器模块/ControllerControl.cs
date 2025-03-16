using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerControl : InputControl_Base, IUpDateInputInformation
{
    protected override void InitializeController()
    {
    }

    protected override void DestroyController()
    {
    }

    void Awake()
    {
        DestroyController();
    }

    void Update()
    {

    }

}
