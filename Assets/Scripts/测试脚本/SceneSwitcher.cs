using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string overlayScene1;
    public string overlayScene2;
    private string currentOverlayScene;
    private SceneItemManager boundaryManager;

    void Start()
    {
        boundaryManager = FindObjectOfType<SceneItemManager>();
        if (boundaryManager == null)
        {
            Debug.LogError("缺少SceneItemManager！");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleScenes();
        }
    }

    void ToggleScenes()
    {
        if (string.IsNullOrEmpty(currentOverlayScene))
        {
            LoadScene(overlayScene1);
        }
        else
        {
            UnloadCurrentScene();
            LoadScene(currentOverlayScene == overlayScene1 ? overlayScene2 : overlayScene1);
        }
    }

    void LoadScene(string sceneName)
    {
        if (SceneExists(sceneName))
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            currentOverlayScene = sceneName;
        }
    }

    void UnloadCurrentScene()
    {
        if (!string.IsNullOrEmpty(currentOverlayScene))
        {
            // 销毁所有不在房间内的物品
            foreach (var item in boundaryManager.allItems.ToArray())
            {
                if (!item.isInRoom)
                {
                    Destroy(item.gameObject);
                    boundaryManager.UnregisterItem(item);
                }
            }
            
            SceneManager.UnloadSceneAsync(currentOverlayScene);
            currentOverlayScene = null;
        }
    }

    bool SceneExists(string sceneName)
    {
        // 同之前的场景存在检查代码
        return true;
    }
}