using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Michsky.LSS
{
    [RequireComponent(typeof(CanvasGroup))]
    public class LoadingScreen : MonoBehaviour
    {
        // ========== 单例实例 ==========
        private static LoadingScreen instance = null; // 单例实例

        // ========== 文字样式设置 ==========
        [Header("文字样式")]
        public float titleSize = 50;                  // 标题字体大小
        public float descriptionSize = 28;             // 描述字体大小
        public float hintSize = 32;                    // 提示字体大小
        public float statusSize = 24;                  // 状态文字大小
        public float pakSize = 35;                     // 按键提示字体大小
        public TMP_FontAsset titleFont;                // 标题字体资源
        public TMP_FontAsset descriptionFont;          // 描述字体资源
        public TMP_FontAsset hintFont;                 // 提示字体资源
        public TMP_FontAsset statusFont;               // 状态字体资源
        public TMP_FontAsset pakFont;                  // 按键提示字体资源

        // ========== 颜色设置 ==========
        [Header("颜色设置")]
        public Color titleColor = Color.white;         // 标题颜色
        public Color descriptionColor = Color.white;   // 描述颜色
        public Color hintColor = Color.white;         // 提示颜色
        public Color spinnerColor = Color.white;       // 加载动画颜色
        public Color statusColor = Color.white;        // 状态文字颜色
        public Color pakColor = Color.white;           // 按键提示颜色

        // ========== 文本内容 ==========
        [Header("文本内容")]
        [TextArea] public string titleObjText = "Title";           // 标题默认文本
        [TextArea] public string titleObjDescText = "Description";  // 描述默认文本
        [TextArea] public string pakText = "Press {KEY} to continue"; // 按键提示模板文本

        // ========== UI组件引用 ==========
        [Header("UI组件")]
        public CanvasGroup canvasGroup;                // 控制整体透明度的组件
        public TextMeshProUGUI statusObj;              // 加载进度百分比文本
        public TextMeshProUGUI titleObj;               // 标题文本组件
        public TextMeshProUGUI descriptionObj;         // 描述文本组件
        public Slider progressBar;                     // 进度条组件
        public Sprite backgroundImage;                 // 默认背景图片
        public Transform spinnerParent;                // 加载动画父节点
        public static string prefabName = "Basic";     // 加载界面预制体名称

        // ========== 提示系统 ==========
        [Header("提示系统")]
        public TextMeshProUGUI hintsText;               // 提示文本组件
        public bool changeHintWithTimer;               // 是否定时更换提示
        public float hintTimerValue = 5;               // 提示更换间隔时间
        private float htvTimer;                        // 提示计时器
        [TextArea] public List<string> hintList = new List<string>(); // 提示内容列表

        // ========== 图片系统 ==========
        [Header("图片系统")]
        public Image imageObject;                     // 背景图片组件
        public Animator fadingAnimator;                // 图片淡入淡出动画控制器
        public List<Sprite> ImageList = new List<Sprite>(); // 随机图片列表
        public bool changeImageWithTimer;              // 是否定时更换背景图
        public float imageTimerValue = 5;              // 图片更换间隔时间
        private float itvTimer;                        // 图片计时器
        [Range(0.1f, 5)] public float imageFadingSpeed = 1; // 图片切换动画速度

        // ========== 按键提示系统 ==========
        [Header("按键提示")]
        public Animator objectAnimator;                // 整体动画控制器
        public TextMeshProUGUI pakTextObj;             // 按键提示文本组件
        public TextMeshProUGUI pakCountdownLabel;      // 倒计时文本
        public Slider pakCountdownSlider;              // 倒计时进度条
#if ENABLE_LEGACY_INPUT_MANAGER
        public KeyCode keyCode = KeyCode.Space;       // 旧版输入系统按键设置
#elif ENABLE_INPUT_SYSTEM
        public InputAction keyCode;                    // 新版输入系统动作
#endif
        public bool useSpecificKey = false;            // 是否使用特定按键
        private bool enableFading = false;             // 是否启用淡出效果
        [Tooltip("Second(s)")] 
        [Range(1, 30)] public int pakCountdownTimer = 5; // 倒计时总时长

        // ========== 虚拟加载系统 ==========
        [Header("虚拟加载")]
        [Tooltip("Second(s)")] 
        public float virtualLoadingTimer = 5;          // 虚拟加载总时长
        private float vltTimer;                        // 虚拟加载计时器
        [Header("虚拟加载覆盖")]
        public static bool overrideLoading = false; // 新增静态变量用于覆盖加载逻辑

        // ========== 功能开关 ==========
        [Header("功能开关")]
        public bool enableVirtualLoading = false;      // 是否启用虚拟加载
        public bool enableTitle = true;                // 是否显示标题
        public bool enableDescription = true;         // 是否显示描述
        public bool enableStatusLabel = true;          // 是否显示进度百分比
        public bool enableRandomHints = true;          // 是否启用随机提示
        public bool enableRandomImages = true;         // 是否启用随机图片
        public bool enablePressAnyKey = true;          // 是否启用按键继续
        [Tooltip("If fading-in is not working properly, you can enable this option.")]
        public bool forceCanvasGroup = false;          // 强制使用CanvasGroup淡入
        [Range(0.1f, 10)] public float fadingAnimationSpeed = 2.0f; // 淡入淡出速度

        // ========== 事件系统 ==========
        [Header("事件系统")]
        public UnityEvent onBeginEvents;               // 加载开始时触发事件
        public UnityEvent onFinishEvents;              // 加载完成时触发事件

        // ========== 内部变量 ==========
        public int spinnerHelper;                     // 加载动画类型辅助变量
        public bool updateHelper = false;              // 更新辅助标记
        private bool onFinishInvoked = false;          // 完成事件触发标记
        //public static bool processLoading = false;     // 加载过程状态标记
        private static bool fcgHelper = false;         // CanvasGroup辅助标记
        private static string sceneHelper;             // 场景名称临时存储

        void Awake()
        {
            // 初始化CanvasGroup组件
            if (canvasGroup == null) { canvasGroup = gameObject.GetComponent<CanvasGroup>(); }
            if (forceCanvasGroup == true) { fcgHelper = true; } // 强制模式标记

            canvasGroup.alpha = 0f; // 初始完全透明
        }

        void Start()
        {
            // 初始化输入系统
#if !ENABLE_LEGACY_INPUT_MANAGER
            keyCode.Enable();
#endif

            // 设置动画更新模式（不受时间缩放影响）
            objectAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            fadingAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

            // 初始化虚拟加载系统
            if (enableVirtualLoading == true) { 
                Debug.Log("<b>[LSS]</b> Virtual loading is enabled. Timer: <b>" + virtualLoadingTimer + "</b>"); 
            }

            // 初始化随机提示系统
            if (enableRandomHints == true) { 
                string hintChose = hintList[Random.Range(0, hintList.Count)]; 
                hintsText.text = hintChose; 
            }
           
            // 初始化随机图片系统
            if (enableRandomImages == true) { 
                Sprite imageChose = ImageList[Random.Range(0, ImageList.Count)]; 
                imageObject.sprite = imageChose; 
            }
            else { 
                fadingAnimator.Play("Wait"); 
                imageObject.sprite = backgroundImage; 
            }

            // 初始化倒计时系统
            if (enablePressAnyKey == true && pakCountdownSlider != null)
            {
                pakCountdownSlider.maxValue = pakCountdownTimer;
                pakCountdownSlider.value = pakCountdownTimer;
            }
      
            // 设置动画速度
            fadingAnimator.speed = imageFadingSpeed;
            statusObj.text = "0%";    // 初始化进度文本
            progressBar.value = 0;    // 初始化进度条
        }

        private AsyncOperation loadingProcess;  // 异步加载操作

        // ========== 核心加载方法 ==========
        public static void LoadScene()
        {
            try
            {
                //processLoading = true;
                
                // 仅生成加载界面
                GameObject prefab = Resources.Load<GameObject>("Loading Screens/" + prefabName);
                instance = Instantiate(prefab).GetComponent<LoadingScreen>();
                instance.gameObject.SetActive(true);
                DontDestroyOnLoad(instance.gameObject);

                // 仅初始化虚拟加载
                instance.enableVirtualLoading = true;
                instance.vltTimer = 0;
                instance.onBeginEvents.Invoke();

                // 强制设置Canvas可见
                instance.canvasGroup.alpha = 1;
            }
            catch { Debug.LogError("<b>[LSS]</b> Prefab加载失败: Resources/Loading Screens/" + prefabName); }
        }

        void Update()
        {
            if (enableVirtualLoading)
            {
                // 虚拟进度控制
                vltTimer += Time.unscaledDeltaTime;
                progressBar.value = Mathf.Clamp01(vltTimer / virtualLoadingTimer);
                statusObj.text = $"{(progressBar.value * 100):F0}%";

                // 虚拟时间结束后直接销毁
                if (vltTimer >= virtualLoadingTimer)
                {
                    enableFading = true;
                    onFinishEvents.Invoke();
                    Destroy(gameObject); // 立即销毁
                }
            }
           

            // 保留其他系统（提示、图片等）
            if (enableRandomImages) ProcessRandomImages();
            if (enableRandomHints) ProcessRandomHints();
        }

        // ========== 异步加载处理核心逻辑 ==========
//         private void ProcessAsyncLoading()
//         {
//             // 强制CanvasGroup模式处理
//             if (processLoading == true && forceCanvasGroup == true)
//             {
//                 canvasGroup.alpha += fadingAnimationSpeed * Time.unscaledDeltaTime;

//                 if (canvasGroup.alpha >= 1) // 完全显示后开始加载
//                 {
//                     forceCanvasGroup = false;
//                     instance.loadingProcess = SceneManager.LoadSceneAsync(sceneHelper);
//                     instance.loadingProcess.allowSceneActivation = false;
//                     fadingAnimator.Play("Fade In");
//                 }
//             }

//             // 虚拟加载模式处理
//             else if (processLoading == true && enableVirtualLoading == true && forceCanvasGroup == false)
//             {
//                 progressBar.value += 1 / virtualLoadingTimer * Time.unscaledDeltaTime;
//                 statusObj.text = Mathf.Round(progressBar.value * 100).ToString() + "%";
//                 vltTimer += Time.unscaledDeltaTime;

//                 if (vltTimer >= virtualLoadingTimer) // 虚拟加载完成
//                 {
//                     if (enablePressAnyKey == true) // 需要按键确认
//                     {
//                         loadingProcess.allowSceneActivation = true;

//                         // 切换至按键提示动画
//                         if (objectAnimator.GetCurrentAnimatorStateInfo(0).IsName("Start")) { 
//                             objectAnimator.Play("Switch to PAK"); 
//                         }
                     
//                         // 处理淡出逻辑
//                         if (enableFading == true) { 
//                             canvasGroup.alpha -= fadingAnimationSpeed * Time.unscaledDeltaTime; 
//                         }
//                         else // 处理倒计时逻辑
//                         {
//                             pakCountdownSlider.value -= 1 * Time.unscaledDeltaTime;
//                             pakCountdownLabel.text = Mathf.Round(pakCountdownSlider.value * 1).ToString();

//                             if (pakCountdownSlider.value == 0) // 倒计时结束
//                             {
//                                 if (onFinishInvoked == false)
//                                 {
//                                     onFinishEvents.Invoke();
//                                     onFinishInvoked = true;
//                                 }
//                                 enableFading = true;
//                                 StartCoroutine("DestroyLoadingScreen");
//                                 canvasGroup.interactable = false;
//                                 canvasGroup.blocksRaycasts = false;
//                             }
//                         }

//                         // 输入检测逻辑
// #if ENABLE_LEGACY_INPUT_MANAGER
//                         if (enableFading == false && useSpecificKey == false && Input.anyKeyDown)
// #elif ENABLE_INPUT_SYSTEM
//                         if (enableFading == false && useSpecificKey == false && Keyboard.current.anyKey.wasPressedThisFrame)
// #endif
//                         {
//                             enableFading = true;
//                             StartCoroutine("DestroyLoadingScreen");
//                             canvasGroup.interactable = false;
//                             canvasGroup.blocksRaycasts = false;
//                             if (onFinishInvoked == false) { 
//                                 onFinishEvents.Invoke(); 
//                                 onFinishInvoked = true; 
//                             }
//                         }

//                         // 特定按键检测
// #if ENABLE_LEGACY_INPUT_MANAGER
//                         else if (enableFading == false && useSpecificKey == true && Input.GetKeyDown(keyCode))
// #elif ENABLE_INPUT_SYSTEM
//                         else if (enableFading == false && useSpecificKey == true && keyCode.triggered)
// #endif
//                         {
//                             enableFading = true;
//                             StartCoroutine("DestroyLoadingScreen");
//                             canvasGroup.interactable = false;
//                             canvasGroup.blocksRaycasts = false;
//                             if (onFinishInvoked == false) { 
//                                 onFinishEvents.Invoke(); 
//                                 onFinishInvoked = true; 
//                             }
//                         }
//                     }
//                     else // 无需按键确认的情况
//                     {
//                         loadingProcess.allowSceneActivation = true;
//                         canvasGroup.alpha -= fadingAnimationSpeed * Time.unscaledDeltaTime;

//                         if (onFinishInvoked == false) { 
//                             onFinishEvents.Invoke(); 
//                             onFinishInvoked = true; 
//                         }
//                         if (canvasGroup.alpha <= 0) { 
//                             Destroy(gameObject); 
//                         }
//                     }
//                 }
//                 else // 加载过程中渐显
//                 {
//                     canvasGroup.alpha += fadingAnimationSpeed * Time.unscaledDeltaTime;
//                     if (canvasGroup.alpha >= 1) { 
//                         loadingProcess.allowSceneActivation = true; 
//                     }
//                 }
//             }

//             // 真实加载模式处理
//             else if (processLoading == true && enableVirtualLoading == false)
//             {
//                 progressBar.value = loadingProcess.progress;
//                 statusObj.text = Mathf.Round(progressBar.value * 100).ToString() + "%";

//                 // 加载完成处理
//                 if (loadingProcess.isDone && enablePressAnyKey == false)
//                 {
//                     canvasGroup.alpha -= fadingAnimationSpeed * Time.unscaledDeltaTime;

//                     if (canvasGroup.alpha <= 0) { 
//                         Destroy(gameObject); 
//                     }
//                     if (onFinishInvoked == false) { 
//                         onFinishEvents.Invoke(); 
//                         onFinishInvoked = true; 
//                     }
//                 }

//                 // 加载未完成处理
//                 else if (!loadingProcess.isDone)
//                 {
//                     canvasGroup.alpha += fadingAnimationSpeed * Time.unscaledDeltaTime;
//                     if (canvasGroup.alpha >= 1) { 
//                         loadingProcess.allowSceneActivation = true; 
//                     }
//                 }

//                 // 需要按键确认的完成处理
//                 if (loadingProcess.isDone && enablePressAnyKey == true)
//                 {
//                     loadingProcess.allowSceneActivation = true;

//                     if (onFinishInvoked == false) { 
//                         onFinishEvents.Invoke(); 
//                         onFinishInvoked = true; 
//                     }
//                     if (fadingAnimator.GetCurrentAnimatorStateInfo(0).IsName("Wait")) { 
//                         objectAnimator.CrossFade("Switch to PAK", 0.25f); 
//                     }

//                     // 淡出处理
//                     if (enableFading == true) { 
//                         canvasGroup.alpha -= fadingAnimationSpeed * Time.unscaledDeltaTime; 
//                     }
//                     else // 倒计时处理
//                     {
//                         pakCountdownSlider.value -= Time.unscaledDeltaTime;
//                         pakCountdownLabel.text = Mathf.Round(pakCountdownSlider.value * 1).ToString();

//                         if (pakCountdownSlider.value == 0)
//                         {
//                             enableFading = true;
//                             StartCoroutine("DestroyLoadingScreen");
//                             canvasGroup.interactable = false;
//                             canvasGroup.blocksRaycasts = false;
//                         }
//                     }

//                     // 输入检测逻辑（同上）
//                     // ...（略，同虚拟加载部分）
//                 }
//             }
//         }

        // ========== 随机图片系统 ==========
        private void ProcessRandomImages()
        {
            if (changeImageWithTimer == true) // 定时更换模式
            {
                itvTimer += Time.unscaledDeltaTime;

                // 触发淡出动画
                if (itvTimer >= imageTimerValue && fadingAnimator.GetCurrentAnimatorStateInfo(0).IsName("Fade In"))
                    fadingAnimator.Play("Fade Out");

                // 更换新图片
                else if (itvTimer >= imageTimerValue && fadingAnimator.GetCurrentAnimatorStateInfo(0).IsName("Wait"))
                {
                    Sprite cloneHelper = imageObject.sprite;
                    Sprite imageChose = ImageList[Random.Range(0, ImageList.Count)];

                    // 避免重复图片
                    if (imageChose == cloneHelper)
                        imageChose = ImageList[Random.Range(0, ImageList.Count)];

                    imageObject.sprite = imageChose;
                    itvTimer = 0.0f;
                }
            }
            else if (changeImageWithTimer == false) // 单次随机模式
            {
                Sprite imageChose = ImageList[Random.Range(0, ImageList.Count)];
                imageObject.sprite = imageChose;
                if (imageObject.color != new Color32(255, 255, 255, 255)) { 
                    imageObject.color = new Color32(255, 255, 255, 255); 
                }
                fadingAnimator.enabled = false;
                enableRandomImages = false;
            }
        }

        // ========== 随机提示系统 ==========
        private void ProcessRandomHints()
        {
            htvTimer += Time.unscaledDeltaTime;

            if (htvTimer >= hintTimerValue) // 到达更换时间
            {
                string cloneHelper = hintsText.text;
                string hintChose = hintList[Random.Range(0, hintList.Count)];
                hintsText.text = hintChose;
                // 避免重复提示
                if (hintChose == cloneHelper) { 
                    hintChose = hintList[Random.Range(0, hintList.Count)]; 
                }
                htvTimer = 0.0f;
            }
        }

        // ========== 销毁加载界面协程 ==========
        IEnumerator DestroyLoadingScreen()
        {
            while (canvasGroup.alpha != 0) { 
                yield return new WaitForSecondsRealtime(0.5f); 
            }
            if (canvasGroup.alpha == 0) { 
                Destroy(gameObject); 
            }
        }
    }
}