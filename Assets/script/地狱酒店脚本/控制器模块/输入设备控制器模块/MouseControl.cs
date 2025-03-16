using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControl : InputControl_Base, IUpDateInputInformation
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
