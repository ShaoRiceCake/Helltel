using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
/// <summary>
/// 音频管理器（单例模式）
/// 核心功能：
/// - 分层音量控制（Master为最高层级）
/// - 音频源对象池管理
/// - 动态随机化处理
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region Singleton
    public static AudioManager Instance { get; private set; }
    #endregion

    #region Configuration
    [Header("基础配置")]
    [SerializeField, Tooltip("音频配置文件")] 
    private AudioConfig config;
    
    [SerializeField, Tooltip("默认音频混合组")] 
    private AudioMixerGroup defaultMixerGroup;
    
    //混响组件
    [SerializeField] private AudioMixer audioMixer;
    #endregion

    #region Volume Control
    [System.Serializable]
    public class SoundCategory
    {
        public AudioCategory type;
        [Range(0, 1)] public float volume = 1f;
    }
    
    [Header("音量分类"), SerializeField]
    // 将数组改为List
    private List<SoundCategory> categories = new List<SoundCategory>
    {
        new SoundCategory{ type = AudioCategory.Master },
        new SoundCategory{ type = AudioCategory.Music },
        new SoundCategory{ type = AudioCategory.SFX },
        new SoundCategory{ type = AudioCategory.Voice }
    };
    #endregion

    #region Runtime Data
    //跟踪音频源的字典
    private Dictionary<AudioSource, (string soundName, Coroutine tracker)> activeSources = new Dictionary<AudioSource, (string, Coroutine)>();
    private Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();
    public List<AudioSource> audioSourcePool = new List<AudioSource>();
    #endregion

    #region Initialization
    private void Awake()
    {
        InitializeSingleton();
        InitializeCategories();
        InitializeAudioPool();
        LoadAudioConfig();
    }

    /// <summary>
    /// 初始化单例实例（跨场景持久化）
    /// </summary>
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 初始化音频源对象池
    /// </summary>
    private void InitializeAudioPool()
    {
        for (int i = 0; i < config.initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }
    }
    //分类初始化方法
    private void InitializeCategories()
    {
        // 强制创建必要分类
        var requiredCategories = new List<AudioCategory> {
            AudioCategory.Master,
            AudioCategory.Music,
            AudioCategory.SFX,
            AudioCategory.Voice
        };

        foreach(var catType in requiredCategories)
        {
            if(!categories.Any(c => c.type == catType))
            {
                categories.Add(new SoundCategory { 
                    type = catType,
                    volume = 1f 
                });
                Debug.LogWarning($"自动创建缺失分类: {catType}");
            }
        }
    }

    /// <summary>
    /// 加载音频配置到字典（自动过滤重复项）
    /// </summary>
    private void LoadAudioConfig()
    {
        foreach (var effect in config.soundEffects)
        {
            if (soundEffects.ContainsKey(effect.name))
            {
                Debug.LogWarning($"发现重复音效名称: {effect.name}");
                continue;
            }
            soundEffects.Add(effect.name, effect);
        }
    }
    #endregion

    #region Core Functionality
    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="soundName">音效配置名称</param>
    /// <param name="position">3D音效播放位置</param>
    /// <param name="dynamicVolume">动态音量系数（如距离衰减）</param>
    /// <param name="loop">是否循环播放</param>
    /// <returns>可控制的AudioSource实例</returns>
    public AudioSource Play(string soundName, 
                          Vector3 position = default, 
                          float dynamicVolume = 1f,
                          bool loop = false)
    {
        if (!soundEffects.TryGetValue(soundName, out SoundEffect effect))
        {
            Debug.Log($"音效不存在: {soundName}");
            return null;
        }

        AudioSource source = GetAvailableAudioSource() ?? CreateNewAudioSource();
        ConfigureAudioSource(source, effect, position, dynamicVolume, loop);
        source.Play();
        // 清理旧的状态跟踪
        if (activeSources.TryGetValue(source, out var oldState))
        {
            StopCoroutine(oldState.tracker);
        }

        // 创建新的状态跟踪
        Coroutine tracker = StartCoroutine(TrackAudioSource(source, soundName, loop));
        activeSources[source] = (soundName, tracker);

        return source;
    }
    // 跟踪方法
    private IEnumerator TrackAudioSource(AudioSource source, string soundName, bool loop)
    {
        if (loop)
        {
            while (source.isPlaying && source.loop)
            {
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(source.clip.length);
            while (source.isPlaying)
            {
                yield return null;
            }
        }

        activeSources.Remove(source);
    }

    /// <summary>
    /// 配置音频源参数（整合所有设置逻辑）
    /// </summary>
    private void ConfigureAudioSource(AudioSource source,
                                     SoundEffect effect,
                                     Vector3 position,
                                     float dynamicVolume,
                                     bool loop)
    {
        // 选择音频剪辑（优先使用随机列表）
        source.clip = SelectRandomClip(effect);
        
        // 分层音量计算（Master > Category > Default > Dynamic）
        float masterVolume = GetCategoryVolume(AudioCategory.Master);
        float categoryVolume = GetCategoryVolume(effect.category);
        float baseVolume = effect.defaultVolume * masterVolume * categoryVolume;
        source.volume = ApplyVolumeVariation(baseVolume * dynamicVolume, effect);

        // 音调设置（包含随机变化）
        source.pitch = CalculatePitch(effect);
        
        // 基础参数
        source.loop = loop;
        
        // 空间音频配置
        ConfigureSpatialAudio(source, effect, position);
    }

    /// <summary>
    /// 随机选择音频剪辑（自动处理空列表）
    /// </summary>
    private AudioClip SelectRandomClip(SoundEffect effect)
    {
        if (effect.randomClips.Count > 0)
        {
            int index = Random.Range(0, effect.randomClips.Count);
            return effect.randomClips[index];
        }
        return effect.clip;
    }

    /// <summary>
    /// 应用音量倍率波动（±10%范围）
    /// </summary>
    private float ApplyVolumeVariation(float baseVolume, SoundEffect effect)
    {
        if (!effect.useVariation || Mathf.Approximately(baseVolume, 0f)) 
            return baseVolume;

        // 生成0.9-1.1的随机倍率
        float multiplier = Random.Range(0.9f, 1.1f);
        return Mathf.Clamp(baseVolume * multiplier, 0f, 1f);
    }

    /// <summary>
    /// 计算音调（包含随机变化）
    /// </summary>
    private float CalculatePitch(SoundEffect effect)
    {
        if (!effect.useVariation) return 1f;
        
        // 生成0.95-1.05的随机倍率
        float multiplier = Random.Range(0.9f, 1.1f);
        return Mathf.Clamp(1f  * multiplier, 0.5f, 2f);
    }

    /// <summary>
    /// 配置空间音频参数（3D音效专用）
    /// </summary>
    private void ConfigureSpatialAudio(AudioSource source, SoundEffect effect, Vector3 position)
    {
        source.spatialBlend = effect.is3D ? 1f : 0f;
        source.outputAudioMixerGroup = effect.mixerGroup ?? defaultMixerGroup;

        if (effect.is3D)
        {
            source.transform.position = position;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
        }
    }
    #endregion

    #region Utility Methods
    /// <summary>
    /// 创建新AudioSource并加入对象池
    /// </summary>
    private AudioSource CreateNewAudioSource()
    {
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        audioSourcePool.Add(newSource);
        return newSource;
    }

    /// <summary>
    /// 获取闲置的AudioSource（自动扩容）
    /// </summary>
    private AudioSource GetAvailableAudioSource()
    {
        foreach (var source in audioSourcePool)
        {
            if (!source.isPlaying) return source;
        }
        return CreateNewAudioSource(); // 自动扩容
    }

    private float GetCategoryVolume(AudioCategory categoryType)
    {
        foreach (var category in categories)
        {
            if (category.type == categoryType)
                return category.volume;
        }
        Debug.LogWarning($"未找到分类: {categoryType}");
        return 1f;
    }
    #endregion

    #region Public Controls
    /// <summary>
    /// 设置主音量
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        SetCategoryVolume(AudioCategory.Master, volume);
    }

    /// <summary>
    /// 设置指定分类的音量
    /// </summary>
    public void SetCategoryVolume(AudioCategory categoryType, float volume)
    {
        foreach (var category in categories)
        {
            if (category.type == categoryType )
            {
                category.volume = Mathf.Clamp01(volume);
                return;
            }
        }
        Debug.LogWarning($"尝试设置不存在的分类: {categoryType}");
    }

    /// <summary>
    /// 停止所有正在播放的音效
    /// </summary>
    public void StopAll()
    {
        foreach (var source in audioSourcePool)
        {
            source.Stop();
        }
    }
    /// <summary>
    /// 切换3D音频功能（未完成
    /// </summary>
    public void Toggle3DAudio(bool enable)
    {
        // 实现具体3D音频开关逻辑
        foreach(var source in audioSourcePool)
        {
            source.spatialBlend = enable ? 1f : 0f;
        }
    }
    /// <summary>
    /// 控制混响效果开关（未完成
    /// </summary>
    /// <param name="enable">true启用混响，false关闭</param>
    public void ToggleReverb(bool enable)
    {
        // 通过音频混合器控制
        audioMixer.SetFloat("ReverbMix", enable ? 0f : -80f); // 0dB启用，-80dB静音
    }
    /// <summary>
    /// 停止指定名称的所有音效实例
    /// </summary>
    public void Stop(string soundName)
    {
        List<AudioSource> toStop = new List<AudioSource>();
        
        foreach (var pair in activeSources)
        {
            if (pair.Value.soundName == soundName && pair.Key.isPlaying)
            {
                toStop.Add(pair.Key);
            }
        }

        foreach (var source in toStop)
        {
            source.Stop();
            if (activeSources.TryGetValue(source, out var state))
            {
                StopCoroutine(state.tracker);
                activeSources.Remove(source);
            }
        }
    }

    /// <summary>
    /// 停止指定的AudioSource实例
    /// </summary>
    //用法演示：
    //AudioSource mySource = AudioManager.Instance.Play("背景音乐", loop: true);
    // ...稍后...
    //AudioManager.Instance.Stop(mySource);  // 停止这个特定的循环音效
    public void Stop(AudioSource source)
    {
        if (source == null) return;

        if (activeSources.TryGetValue(source, out var state))
        {
            source.Stop();
            StopCoroutine(state.tracker);
            activeSources.Remove(source);
        }
    }
    #endregion

   
}