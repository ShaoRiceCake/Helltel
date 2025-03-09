using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class Obigrouptest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // ��ȡ�������
        ObiSoftbody softbody = this.GetComponent<ObiSoftbody>();

        // ��������Ԥ��������
        foreach (ObiParticleGroup group in softbody.blueprint.groups)
        {
            Debug.Log($"����: {group.name}, ������: {group.particleIndices.Count}");
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
