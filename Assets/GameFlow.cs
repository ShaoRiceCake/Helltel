using System;
using System.Collections;
using System.Collections.Generic;
using Agora.Rtc;
using UnityEngine;

public class GameFlow : MonoBehaviour
{
    private GameDataModel _data;
    public SceneOverlayer sceneOverlayer;


 

    void Start()
    {
        _data = GameController.Instance._gameData;
        sceneOverlayer = FindObjectOfType<SceneOverlayer>();
        
        StartCoroutine(NextFloor());
        
               
        
        
    }

    // Update is called once per frame
    void Update()
    {
       
        
    }
    IEnumerator NextFloor()
    {
        sceneOverlayer.SwitchScenes();
        yield return new WaitForSecondsRealtime(0.1f);
        //如果电梯外面是地牢
        if(_data.CurrentLoadedScene != null &&_data.CurrentLoadedScene == _data.dungeon)
        {
            GameController.Instance._gameData.Level +=1;
            //这段代码会设置地牢生成的值并重新调用地牢生成
            DungeonGenerator.Instance.ReSetDungeonValue();
        }
        else if(_data.CurrentLoadedScene != null && _data.CurrentLoadedScene == _data.shop)
        {

        }

    }

    internal class Instance
    {
    }
}
