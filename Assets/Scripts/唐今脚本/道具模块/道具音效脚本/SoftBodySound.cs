using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoftBodySound : ItemSoundBase
{
    protected override void Start()
    {
        base.Start();
        soundName = "软体物体碰撞常规音效";
    }
}
