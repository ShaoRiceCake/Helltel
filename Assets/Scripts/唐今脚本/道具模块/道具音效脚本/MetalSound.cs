using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalSound : ItemSoundBase
{
    protected override void Start()
    {
        base.Start();
        soundName = "金属物体碰撞常规音效";
    }
}
