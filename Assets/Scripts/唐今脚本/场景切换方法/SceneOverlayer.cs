using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Netcode;
using System;

public class SceneOverlayer : NetworkBehaviour
{
    public GameDataModel _data;

    
    
    [SerializeField] private bool _isScene1Active;
    [SerializeField] private bool ready=true;
    private void Start()
    {
        _data = GameController.Instance._gameData;

    }

    private void Update()
    {

        if (NetworkManager.Singleton)
        {
            if (!GameManager.instance.isGameing) return;
            if (!ready) return;
            if (IsHost)
            {
                if (Input.GetKeyDown(KeyCode.L))
                {
                    SwitchScenes();
                }
            }

        }
        else
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                SwitchScenes();
            }
        }

    }

    public void SwitchScenes()
    {
        ready = false;
        Debug.Log("我执行了5");
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

        if (_isScene1Active)
        {
            if (NetworkManager.Singleton)
            {
                Invoke(nameof(LoadShopScene), 0.2f);
            }
            else
            {
                SceneManager.LoadScene(_data.shop, LoadSceneMode.Additive);
            }
           
            _data.CurrentLoadedScene = _data.shop;
            _isScene1Active = false;
        }
        else
        {
            if (NetworkManager.Singleton)
            {
                Invoke(nameof(LoadDungeonScene), 0.2f);
            }
            else
            {
                SceneManager.LoadScene(_data.dungeon, LoadSceneMode.Additive);
            }

            _data.CurrentLoadedScene = _data.dungeon;
            _isScene1Active = true;
        }
    }

    void LoadDungeonScene()
    {
        GameManager.instance.LoadSceneAddtive(_data.dungeon);
        Debug.Log("加载地牢");
        ready = true ;
    }

    void LoadShopScene()
    {
        GameManager.instance.LoadSceneAddtive(_data.shop);
        Debug.Log("加载商店");
        ready = true;
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
