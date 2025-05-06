using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Netcode;
using System;
using Michsky.LSS;
using Agora.Rtc;

public class SceneOverlayer : NetworkBehaviour
{
    private GameDataModel _data;
    
    
    
    //[SerializeField] private bool _isScene1Active;
    [SerializeField] private bool ready=true;
    private void Start()
    {
        _data = GameController.Instance._gameData;

    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.K))
        {
            SwitchToDungeon();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            SwitchToShop();
        }

    }

    // public void SwitchScenes()
    // {
    //     //ready = false;
    //     // if (!string.IsNullOrEmpty(_data.CurrentLoadedScene))
    //     // {
    //     //     if (NetworkManager.Singleton)
    //     //     {
    //     //         GameManager.instance.UnLoadScene(_data.CurrentLoadedScene);
    //     //         Debug.Log("卸载" + _data.CurrentLoadedScene);
    //     //     }
    //     //     else
    //     //     {
    //     //         SceneManager.UnloadSceneAsync(_data.CurrentLoadedScene);
    //     //     }

    //     // }

    //     // if (_data.CurrentLoadedScene == _data.dungeon)
    //     // {
    //     //     if (NetworkManager.Singleton)
    //     //     {
    //     //         Invoke(nameof(LoadShopScene), 0.2f);
    //     //     }
    //     //     else
    //     //     {
    //     //         lSS_Manager.LoadScene(_data.shop);
    //     //         //SceneManager.LoadScene(_data.shop, LoadSceneMode.Additive);
    //     //     }
           
    //     //     _data.CurrentLoadedScene = _data.shop;
    //     //     _isScene1Active = false;
    //     // }
    //     // else
    //     // {
    //     //     if (NetworkManager.Singleton)
    //     //     {
    //     //         Invoke(nameof(LoadDungeonScene), 0.2f);
    //     //     }
    //     //     else
    //     //     {
    //     //         lSS_Manager.LoadScene(_data.shop);
    //     //         //SceneManager.LoadScene(_data.dungeon, LoadSceneMode.Additive);
    //     //     }

    //     //     _data.CurrentLoadedScene = _data.dungeon;
    //     //     _isScene1Active = true;
    //     // }
    // }
    public void SwitchToDungeon()
    {
        SceneManager.UnloadSceneAsync(_data.CurrentLoadedScene);
        GameController.Instance.lSS_Manager.LoadScene();
        SceneManager.LoadScene(_data.dungeon, LoadSceneMode.Additive);
        _data.CurrentLoadedScene = _data.dungeon;
    }
    public void SwitchToShop()
    {
        SceneManager.UnloadSceneAsync(_data.CurrentLoadedScene);
        GameController.Instance.lSS_Manager.LoadScene();
        SceneManager.LoadScene(_data.shop, LoadSceneMode.Additive);
        _data.CurrentLoadedScene = _data.shop;
    }
    public void LoadDungeonScene()
    {
        SceneManager.LoadScene(_data.dungeon, LoadSceneMode.Additive);
        _data.CurrentLoadedScene = _data.dungeon;
        
    }

    public void LoadShopScene()
    {
        SceneManager.LoadScene(_data.shop, LoadSceneMode.Additive);
        _data.CurrentLoadedScene = _data.shop;
        
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(_data.CurrentLoadedScene))
        {
            if (NetworkManager.Singleton)
            {
                GameManager.instance.UnLoadScene(_data.CurrentLoadedScene);
                Debug.Log("卸载" + _data.CurrentLoadedScene);
            }
            else
            {
                SceneManager.UnloadSceneAsync(_data.CurrentLoadedScene);
            }

        }
    }
    
    public string GetCurrentLoadedSceneName()
    {
        return _data.CurrentLoadedScene;
    }
}
