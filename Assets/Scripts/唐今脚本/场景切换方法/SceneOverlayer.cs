using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneOverlayer : MonoBehaviour
{
    [Tooltip("第一个外部场景（Scene1）")]
    public string scene1Name;
    
    [Tooltip("第二个外部场景（Scene2）")]
    public string scene2Name;

    private string _currentLoadedScene = "";
    private bool _isScene1Active;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SwitchScenes();
        }
    }

    private void SwitchScenes()
    {
        if (!string.IsNullOrEmpty(_currentLoadedScene))
        {
            SceneManager.UnloadSceneAsync(_currentLoadedScene);
        }

        if (_isScene1Active)
        {
            SceneManager.LoadScene(scene2Name, LoadSceneMode.Additive);
            _currentLoadedScene = scene2Name;
            _isScene1Active = false;
        }
        else
        {
            SceneManager.LoadScene(scene1Name, LoadSceneMode.Additive);
            _currentLoadedScene = scene1Name;
            _isScene1Active = true;
        }
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(_currentLoadedScene))
        {
            SceneManager.UnloadSceneAsync(_currentLoadedScene);
        }
    }
    
    public string GetCurrentLoadedSceneName()
    {
        return _currentLoadedScene;
    }
}
