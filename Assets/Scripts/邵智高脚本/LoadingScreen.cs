using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;
    //提示：通过加载场景切换场景时，可以使用类似的代码
    // public void StartGame()
    // {
    //     // 设置目标场景名称
    //     LoadingScreen.SceneLoader.TargetSceneName = "Level1";
        
    //     // 加载加载场景（单例模式）
    //     SceneManager.LoadScene("LoadingScene", LoadSceneMode.Single);
    // }
public class LoadingScreen : MonoBehaviour 
{
    
    [Header("场景配置")]
    private string loadingSceneName = "Loading";
    private float minDisplayTime = 100f;
    [Header("两侧设置")]
    [SerializeField] RectTransform[] foreground;  // 两张无缝拼接的前景
    [SerializeField] RectTransform[] ironChain;  // 两张无缝拼接的前景
    float scrollSpeed = 800f;
    [Header("齿轮设置")]
    [SerializeField] Transform gearUI,gear1,gear2,gear3,gear4;                 // 齿轮Transform
    float gearRotationSpeed = 200f;  // 旋转速度（度/秒）

    private float foregroundHeight,ironChainHeight;

    private Vector2 screenSize =new Vector2(1920,1080);





    // 静态场景参数传递类（跨场景数据容器）
    public static class SceneLoader 
    {
        /// <summary>
        /// 要加载的目标场景名称
        /// </summary>
        public static string TargetSceneName { get; set; }
    }

    private void Start()
    {
        // // 参数校验：确保目标场景名称已设置
        // if (string.IsNullOrEmpty(SceneLoader.TargetSceneName))
        // {
        //     Debug.LogError("目标场景名称未设置！");
        //     SceneManager.LoadScene("MainMenu"); // 退回安全场景
        //     return;
        // }
        
        // 初始化参数
        InitializeAnimations();
        // imageHeight = images[0].rect.height;
        // //screenSize = GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta;
        
        // // 设置初始位置
        // images[0].anchoredPosition = Vector2.zero;
        // images[1].anchoredPosition = new Vector2(0, -imageHeight);
        // // 启动动画
        // StartScroll();

        // 启动异步加载协程
        StartCoroutine(LoadTargetScene(SceneLoader.TargetSceneName));
    }
    void InitializeAnimations()
    {
        StartGearRotation(gearUI,true);
        StartGearRotation(gear1,true);
        StartGearRotation(gear2,false);
        StartGearRotation(gear3,true);
        StartGearRotation(gear4,false);
        StartBackgroundScroll();
    }

    private IEnumerator LoadTargetScene(string targetScene)
    {
        float startTime = Time.realtimeSinceStartup; // 记录加载开始时间
        // 异步加载目标场景（叠加模式）
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
        
        // 禁用自动激活，确保完全加载后再切换
        asyncLoad.allowSceneActivation = false;

        // 加载监控循环
        while (!asyncLoad.isDone)
        {
            /* 
             * Unity的加载进度逻辑：
             * - 0.0~0.9：实际加载进度
             * - 0.9~1.0：等待allowSceneActivation激活
             */
            if (asyncLoad.progress >= 0.9f)
            {
                // 加载完成90%后自动激活场景
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // 计算实际加载耗时
        float loadDuration = Time.realtimeSinceStartup - startTime;
        ///float minDisplayTime = 1.5f; // 最小展示时间（秒）
        
        // 若加载过快，等待补足时间。防止加载场景一闪而过
        if (loadDuration < minDisplayTime) {
            yield return new WaitForSeconds(minDisplayTime - loadDuration);
        }

        // 关键修复：设置新场景为活跃场景
        Scene targetSceneObj = SceneManager.GetSceneByName(targetScene);
        if (targetSceneObj.IsValid())
        {
            SceneManager.SetActiveScene(targetSceneObj);
        }
        else
        {
            Debug.LogError("目标场景无效: " + targetScene);
            yield break;
        }

        // 安全卸载加载场景
        Scene loadingSceneObj = SceneManager.GetSceneByName(loadingSceneName);
        if (loadingSceneObj.IsValid() && loadingSceneObj.isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync(loadingSceneObj);
        }
        else
        {
            Debug.LogError("无法卸载场景: " + loadingSceneName);
        }
    }
    void StartGearRotation(Transform image,bool isClockwise)
    {
        if (image == null) return;

        if(isClockwise == true)
        {
            // 持续顺时针旋转
            image.DOLocalRotate(new Vector3(0, 0, -360), 360 / gearRotationSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1)
            .SetRelative();
        }
        else
        {
            // 持续顺时针旋转
            image.DOLocalRotate(new Vector3(0, 0, 360), 360 / gearRotationSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1)
            .SetRelative();
        }
        
    }

    void StartBackgroundScroll()
    {
        if (foreground.Length != 2 ||ironChain.Length !=2) return;

        foregroundHeight = foreground[0].rect.height;
        ironChainHeight = ironChain[0].rect.height;
        
        // 设置初始位置
        foreground[0].anchoredPosition = Vector2.zero;
        foreground[1].anchoredPosition = new Vector2(0, -foregroundHeight);
        ironChain[0].anchoredPosition = Vector2.zero;
        ironChain[1].anchoredPosition = new Vector2(0, -ironChainHeight);
        
        foreach (var img in foreground)
        {
            img.DOLocalMoveY(foregroundHeight * 1, foregroundHeight / 800)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .OnStepComplete(() => ResetForegroundPosition(img))
                .SetRelative();
        }
        foreach (var img in ironChain)
        {
            img.DOLocalMoveY(ironChainHeight * 1, ironChainHeight / 200)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .OnStepComplete(() => ResetIronChainPosition(img))
                .SetRelative();
        }
    }

    
    void ResetForegroundPosition(RectTransform currentImg)
    {
        var otherImg = currentImg == foreground[0] ? foreground[1] : foreground[0];
        currentImg.anchoredPosition = otherImg.anchoredPosition - new Vector2(0, foregroundHeight);
    }
    void ResetIronChainPosition(RectTransform currentImg)
    {
        var otherImg = currentImg == ironChain[0] ? ironChain[1] : ironChain[0];
        currentImg.anchoredPosition = otherImg.anchoredPosition - new Vector2(0, ironChainHeight);
    }

    void OnDestroy()
    {
        DOTween.KillAll();
    }

}