using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class TempEffectOndestroy : MonoBehaviour
{
    public GameObject effectPrefab; // 预制体引用
    private GameObject effectInstance; // 实例化的预制体引用

    private void Awake()
    {
        effectPrefab.transform.localScale = new Vector3(2, 2, 2);
    }
    private void OnDestroy()
    {
        if (effectPrefab == null) return;
        
        var fx = Instantiate(effectPrefab, transform.position, Quaternion.identity);
        
        fx.transform.localScale = new Vector3(2, 2, 2);
            
        Destroy(fx, 1f);
    }

}
