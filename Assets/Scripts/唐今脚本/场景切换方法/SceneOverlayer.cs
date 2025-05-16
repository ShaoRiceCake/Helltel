using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Netcode;
using System;
using Michsky.LSS;
using Agora.Rtc;

public class SceneOverlayer : MonoBehaviour
{
    private GameDataModel _data;

    // [SerializeField] private bool ready=true;
    public void Awake()
    {
        _data = GameController.Instance._gameData;
    }

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
        if (string.IsNullOrEmpty(_data.CurrentLoadedScene)) return;
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
    
    public string GetCurrentLoadedSceneName()
    {
        return _data.CurrentLoadedScene;
    }
}
