using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Netcode;
using System;

public class SceneOverlayer : NetworkBehaviour
{
    [Tooltip("第一个外部场景（Scene1）")]
    public string dungeon;
    
    [Tooltip("第二个外部场景（Scene2）")]
    public string shop;

    private string _currentLoadedScene = "";
    public string CurrentLoadedScene {get => _currentLoadedScene;}
    [SerializeField] private bool _isScene1Active;
    [SerializeField] private bool ready=true;

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

        if (!string.IsNullOrEmpty(_currentLoadedScene))
        {
            if (NetworkManager.Singleton)
            {
                GameManager.instance.UnLoadScene(_currentLoadedScene);
                Debug.Log("卸载" + _currentLoadedScene);
            }
            else
            {
                SceneManager.UnloadSceneAsync(_currentLoadedScene);
            }

        }

        if (_isScene1Active)
        {
            if (NetworkManager.Singleton)
            {
                Invoke(nameof(LoadScene2), 0.2f);
            }
            else
            {
                SceneManager.LoadScene(shop, LoadSceneMode.Additive);
            }

            _currentLoadedScene = shop;
            _isScene1Active = false;
        }
        else
        {
            if (NetworkManager.Singleton)
            {
                Invoke(nameof(LoadScene1), 0.2f);
            }
            else
            {
                SceneManager.LoadScene(dungeon, LoadSceneMode.Additive);
            }

            _currentLoadedScene = dungeon;
            _isScene1Active = true;
        }
    }

    void LoadScene1()
    {
        GameManager.instance.LoadSceneAddtive(dungeon);
        Debug.Log("加载联机场景1");
        ready = true ;
    }

    void LoadScene2()
    {
        GameManager.instance.LoadSceneAddtive(shop);
        Debug.Log("加载联机场景2");
        ready = true;
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(_currentLoadedScene))
        {
            if (NetworkManager.Singleton)
            {
                GameManager.instance.UnLoadScene(_currentLoadedScene);
                Debug.Log("卸载" + _currentLoadedScene);
            }
            else
            {
                SceneManager.UnloadSceneAsync(_currentLoadedScene);
            }

        }
    }
    
    public string GetCurrentLoadedSceneName()
    {
        return _currentLoadedScene;
    }
}
