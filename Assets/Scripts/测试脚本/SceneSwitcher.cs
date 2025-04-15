using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [Tooltip("输入要切换的场景名称")]
    public string targetSceneName = "Scene2"; // 默认场景名称

    void Update()
    {
        // 检测空格键按下
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadTargetScene();
        }
    }

    public void LoadTargetScene()
    {
        // 检查场景是否存在
        if (SceneExists(targetSceneName))
        {
            Debug.Log("正在加载场景: " + targetSceneName);
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"场景 '{targetSceneName}' 不存在！请检查场景名称是否正确，并确保场景已添加到构建设置中。");
        }
    }

    // 检查场景是否存在于构建设置中
    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            var lastSlash = scenePath.LastIndexOf("/") + 1;
            var sceneNameInBuild = scenePath.Substring(lastSlash, scenePath.LastIndexOf(".") - lastSlash);
            
            if (sceneNameInBuild == sceneName)
                return true;
        }
        return false;
    }
}