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
    private float minDisplayTime = 3f;

    [Header("两侧设置")]
    [SerializeField] RectTransform[] foreground;  // 两张无缝拼接的前景
    [SerializeField] RectTransform[] ironChain;  // 两张无缝拼接的前景
    float scrollSpeed = 800f;
    [Header("齿轮设置")]
    [SerializeField] Transform gearUI,gear1,gear2,gear3,gear4;                 // 齿轮Transform
    float gearRotationSpeed = 200f;  // 旋转速度（度/秒）
    [Header("电梯设置")]
    [SerializeField] Transform elevator;          // 电梯主体Transform
    float shakeStrength = 0.4f; // 震动强度系数
    float shakeDuration = 0.25f;  // 单次震动周期

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
        StartElevatorAnimation();
        StartBackgroundScroll();
    }

    private IEnumerator LoadTargetScene(string targetScene)
    {
        float startTime = Time.realtimeSinceStartup; // 记录加载开始时间
        // 异步加载目标场景（叠加模式）
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
        
        // 禁用自动激活，确保完全加载后再切换
        asyncLoad.allowSceneActivation = false;

        // 第一阶段：等待加载至90%
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // 第二阶段：计算强制等待时间
        float elapsed = Time.realtimeSinceStartup - startTime;
        float remainingWait = Mathf.Max(minDisplayTime - elapsed, 0);
        yield return new WaitForSeconds(remainingWait); // 关键点：先等待再激活
        
        // 第三阶段：激活场景
        asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone)
        {
            yield return null;
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
    void StartElevatorAnimation()
    {
        if (elevator == null) return;

        // 复合震动动画序列
        Sequence elevatorSequence = DOTween.Sequence()
            // 缩放动画：轻微放大后恢复
            .Append(elevator.DOScale(1.01f * Vector3.one, shakeDuration * 0.2f).SetEase(Ease.OutQuad))
            .Append(elevator.DOScale(Vector3.one, shakeDuration * 0.3f).SetEase(Ease.InQuad))
            
            // 位移动画：随机方向晃动
            .Join(elevator.DOShakePosition(
                duration: shakeDuration,
                strength: new Vector3(shakeStrength * 4, shakeStrength * 2, 0),
                vibrato: 10,
                fadeOut: false
            ).SetEase(Ease.Linear))
            
            // 无限循环
            .SetLoops(-1)
            .SetLink(gameObject);

        // 添加随机性参数
        elevatorSequence.timeScale = Random.Range(0.8f, 1.2f); // 速度随机变化
    }

    void OnDestroy()
    {
        DOTween.KillAll();
    }

}