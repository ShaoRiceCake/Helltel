using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class Obigrouptest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 获取软体组件
        ObiSoftbody softbody = this.GetComponent<ObiSoftbody>();

        // 遍历所有预设粒子组
        foreach (ObiParticleGroup group in softbody.blueprint.groups)
        {
            Debug.Log($"组名: {group.name}, 粒子数: {group.particleIndices.Count}");
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
