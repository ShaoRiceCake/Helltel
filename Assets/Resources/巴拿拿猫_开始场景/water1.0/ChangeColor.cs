using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    public object water1;
    public Material waterone;
    void Start()
    {
        InvokeRepeating("ChangeModelColor", 1, 1);

    }

    void ChangeModelColor()
    {
        GetComponent<Renderer>().material.color = Random.ColorHSV(0f, 1f, 0f, 1f, 1, 1);
    }
}
