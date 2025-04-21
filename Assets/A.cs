using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class A : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Change()
    {
        // 设置目标场景名称
        LoadingScreen.SceneLoader.TargetSceneName = "B";
            
        // 加载加载场景（单例模式）
        SceneManager.LoadScene("Loading", LoadSceneMode.Single);    
    }
}
