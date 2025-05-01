using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlow : MonoBehaviour
{
    public SceneOverlayer sceneOverlayer;
    // Start is called before the first frame update
    void Start()
    {
        sceneOverlayer = FindObjectOfType<SceneOverlayer>();
        sceneOverlayer.SwitchScenes();
        Invoke("NextFloor", 0.2f); 
        
               
        
        
    }

    // Update is called once per frame
    void Update()
    {
       
        
    }
    void NextFloor()
    {
       
        //如果电梯外面是地牢
        if(sceneOverlayer.CurrentLoadedScene == sceneOverlayer.dungeon)
        {
            GameController.Instance._gameData.Level +=1;
            DungeonGenerator.Instance.ReSetDungeonValue();
        }
        else
        {

        }

    }
}
