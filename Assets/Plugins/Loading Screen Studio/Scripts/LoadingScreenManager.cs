using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Michsky.LSS
{
    public class LoadingScreenManager : MonoBehaviour
    {
        // ========== 加载设置 ==========
        [Header("加载设置")]
        public LoadingMode loadingMode;             // 加载模式：单场景替换或叠加加载
        public string prefabHelper = "Standard";    // 要使用的加载界面预制体名称
        public bool enableTrigger;                  // 是否启用触发器加载功能
        public bool onTriggerExit;                  // 是否在离开触发器时触发加载（否则进入时触发）
        public bool onlyLoadWithTag;                // 是否只允许特定标签对象触发加载
        public bool startLoadingAtStart;            // 是否在游戏启动时自动开始加载
        public string objectTag;                    // 允许触发加载的物体标签
        public string sceneName;                    // 要加载的目标场景名称

        // ========== 临时变量 ==========
        [Header("临时变量")]
        public Object[] loadingScreens;            // 可选的加载界面预制体数组
        public int selectedLoadingIndex = 0;        // 当前选择的加载界面索引
        public int selectedTagIndex = 0;            // 当前选择的标签索引

        // ========== 事件系统 ==========
        [Header("事件")]
        public UnityEvent onLoadStart;              // 加载开始时触发的事件
        public List<GameObject> dontDestroyOnLoad = new List<GameObject>(); // 加载时保留的对象列表

        // ========== 叠加加载专用 ==========
        [Header("叠加加载")]
        [SerializeField] 
        private List<string> loadedScenes = new List<string>(); // 已加载的叠加场景列表（仅编辑器可见）

        // 加载模式枚举
        public enum LoadingMode { Single, Additive }

        void Start()
        {
            // 如果设置启动时加载且为单场景模式，立即加载目标场景
            if (startLoadingAtStart == true && loadingMode == LoadingMode.Single) 
            { 
                LoadScene(); 
            }
        }

        // 设置加载界面样式的方法
        public void SetStyle(string styleName)
        {
            prefabHelper = styleName;
        }

        // 单场景加载方法
        public void LoadScene()
        {
            LoadingScreen.overrideLoading = true; // 启用覆盖
            LoadingScreen.prefabName = prefabHelper;
            LoadingScreen.LoadScene();

            for (int i = 0; i < dontDestroyOnLoad.Count; i++)
                DontDestroyOnLoad(dontDestroyOnLoad[i]);

            onLoadStart.Invoke();
        }

        // 叠加加载场景方法
        public void LoadAdditiveScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
#if UNITY_EDITOR
            // 编辑器模式下维护已加载场景列表
            loadedScenes.Add(SceneManager.GetSceneByName(sceneName).name);
#endif
        }

        // 卸载叠加场景方法
        public void UnloadAdditiveScene(string sceneName)
        {
            SceneManager.UnloadSceneAsync(sceneName);
#if UNITY_EDITOR
            // 编辑器模式下更新已加载场景列表
            loadedScenes.Remove(sceneName);
#endif
        }

        // 触发器进入事件处理
        private void OnTriggerEnter(Collider other)
        {
            if (loadingMode == LoadingMode.Additive || enableTrigger == false)
                return;

            LoadingScreen.prefabName = prefabHelper;

            // 处理需要标签验证的触发加载
            if (onlyLoadWithTag == true && onTriggerExit == false && other.gameObject.tag == objectTag)
            {
                onLoadStart.Invoke();
                LoadingScreen.LoadScene();
            }
            // 处理无标签验证的触发加载
            else if (onTriggerExit == false) 
            { 
                onLoadStart.Invoke(); 
                LoadingScreen.LoadScene(); 
            }
        }

        // 触发器退出事件处理
        private void OnTriggerExit(Collider other)
        {
            if (loadingMode == LoadingMode.Additive || enableTrigger == false)
                return;

            LoadingScreen.prefabName = prefabHelper;

            // 处理需要标签验证的退出加载
            if (onlyLoadWithTag == true && onTriggerExit == true && other.gameObject.tag == objectTag)
            {
                LoadingScreen.LoadScene();
                onLoadStart.Invoke();
            }
            // 处理无标签验证的退出加载
            else if (onlyLoadWithTag == false && onTriggerExit == true) 
            { 
                LoadingScreen.LoadScene(); 
                onLoadStart.Invoke(); 
            }
        }
    }
}