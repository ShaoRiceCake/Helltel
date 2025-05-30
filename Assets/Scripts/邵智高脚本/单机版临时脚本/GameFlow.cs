using System;
using System.Collections;
using System.Collections.Generic;
using Agora.Rtc;
using UnityEngine;

public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance { get; private set; }
    private GameDataModel _data;
    public SceneOverlayer sceneOverlayer;
    
    private void Awake() {
        if (!Instance) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
        
    }
    private void Start()
    {
        _data = GameController.Instance._gameData;
        sceneOverlayer = FindObjectOfType<SceneOverlayer>();
        sceneOverlayer.LoadDungeonScene();
        _data.SendStartLoading();
        StartCoroutine(NextFloor());
    }
    private IEnumerator NextFloor()
    {
        //如果电梯外面是地牢
        if(_data.CurrentLoadedScene != null &&_data.CurrentLoadedScene == _data.dungeon)
        {
            GameController.Instance._gameData.Level +=1;
            //这段代码会设置地牢生成的值并重新调用地牢生成
            yield return new WaitForSecondsRealtime(0.1f);
            DungeonGenerator.Instance.ReSetDungeonValue();
        }
        else if(_data.CurrentLoadedScene != null && _data.CurrentLoadedScene == _data.shop)
        {

        }
    }
    public void OpenDoor()
    {
        //开电梯门
        AudioManager.Instance.Play("电梯门开");
    }
    public void CloseDoor()
    {
        //关电梯门
        AudioManager.Instance.Play("电梯门关");
    }
    //离开本层要做的事
    public void LeaveThisFloor()
    {
        //关门
        CloseDoor();
        //打开加载界面
        GameController.Instance.lSS_Manager.LoadScene();
        //修改玩家位置；
        //ResetPlayerPosition();
        
        //发送事件，接受到这个事件的地方要进行相应操作（重置属性、位置等）
        _data.SendOnFloorChangedEvent();
        
        //切换叠加的场景
        if(_data.CurrentLoadedScene == _data.dungeon)
        {
            sceneOverlayer.SwitchToShop();
            //更新玩家数据
            _data.NewShopFloorData();
        }
        else if(_data.CurrentLoadedScene == _data.shop)
        {
            sceneOverlayer.SwitchToDungeon();
            //更新玩家数据
            _data.NewDungeonFloorData();
            StartCoroutine(NextFloor());
        }
    }
}
