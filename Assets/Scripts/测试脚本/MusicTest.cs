using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            AudioManager.Instance.Play("擦除", transform.position, 1f);
        }
    }
}
