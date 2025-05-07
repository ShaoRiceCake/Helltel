using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodSound : ItemSoundBase
{
    protected override void Start()
    {
        base.Start();
        soundName = "木制物体碰撞常规音效";
    }
}
