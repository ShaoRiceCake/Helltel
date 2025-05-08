using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class LoadingScreenAni : MonoBehaviour 
{
    
    [Header("场景配置")]
    private string loadingSceneName = "Loading";
    private float minDisplayTime = 20f;

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
        AudioManager.Instance.Play("加载",loop:true);

        // imageHeight = images[0].rect.height;
        // //screenSize = GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta;
        
        // // 设置初始位置
        // images[0].anchoredPosition = Vector2.zero;
        // images[1].anchoredPosition = new Vector2(0, -imageHeight);
        // // 启动动画
        // StartScroll();

       
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

    void StartGearRotation(Transform image,bool isClockwise)
    {
        if (image == null) return;

        if(isClockwise == true)
        {
            // 持续顺时针旋转
            image.DOLocalRotate(new Vector3(0, 0, -360), 360 / gearRotationSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1)
            .SetRelative()
            .SetUpdate(true);
        }
        else
        {
            // 持续顺时针旋转
            image.DOLocalRotate(new Vector3(0, 0, 360), 360 / gearRotationSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1)
            .SetRelative()
            .SetUpdate(true);
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
                .SetRelative()
                .SetUpdate(true);
        }
        foreach (var img in ironChain)
        {
            img.DOLocalMoveY(ironChainHeight * 1, ironChainHeight / 200)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .OnStepComplete(() => ResetIronChainPosition(img))
                .SetRelative()
                .SetUpdate(true);
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

        Sequence elevatorSequence = DOTween.Sequence()
            // 缩放动画
            .Append(elevator.DOScale(1.01f * Vector3.one, shakeDuration * 0.2f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)) 
            .Append(elevator.DOScale(Vector3.one, shakeDuration * 0.3f)
                .SetEase(Ease.InQuad)
                .SetUpdate(true))
            
            // 震动动画
            .Join(elevator.DOShakePosition(
                duration: shakeDuration,
                strength: new Vector3(shakeStrength * 4, shakeStrength * 2, 0),
                vibrato: 10,
                fadeOut: false
            ).SetEase(Ease.Linear)
            .SetUpdate(true)) 
            
            // 循环设置
            .SetLoops(-1)
            .SetUpdate(true) // 整个序列使用真实时间
            .SetLink(gameObject);

        elevatorSequence.timeScale = Random.Range(0.8f, 1.2f);
    }

    void OnDestroy()
    {
        AudioManager.Instance.StopAll();
        DOTween.KillAll();
    }

}