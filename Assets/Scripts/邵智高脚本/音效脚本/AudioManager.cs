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
    //轮播
    private class CyclePlaybackInfo
    {
        public SoundEffect effect;
        public int currentIndex;
        public Vector3 position;
        public float dynamicVolume;
        public AudioSource nextSource; // 预加载的下一个音源
        public float transitionProgress; // 过渡进度
    }
    private Dictionary<AudioSource, CyclePlaybackInfo> activeCycleSources = new Dictionary<AudioSource, CyclePlaybackInfo>();

    #region Runtime Data
    private Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();
    public List<AudioSource> audioSourcePool = new List<AudioSource>();
    // 新增播放上下文记录
    private Dictionary<AudioSource, PlaybackContext> playbackContexts = 
        new Dictionary<AudioSource, PlaybackContext>();
    // 上下文数据结构
    private class PlaybackContext
    {
        public string soundName;
        public MonoBehaviour owner;
    }
    private Dictionary<AudioSource, string> activeSounds = new Dictionary<AudioSource, string>();
    #endregion

    #region Initialization
    private void Awake()
    {
        InitializeSingleton();
        InitializeCategories();
        InitializeAudioPool();
        LoadAudioConfig();
    }
    private void Update()
    {
        HandleCyclePlayback();
        CleanInactiveSources();
        CleanInvalidContexts();
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
    // 分类初始化方法
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
                          bool loop = false,
                          MonoBehaviour owner = null)
    {
        if (!soundEffects.TryGetValue(soundName, out SoundEffect effect))
        {
            Debug.Log($"音效不存在: {soundName}");
            return null;
        }

        AudioSource source = GetAvailableAudioSource() ?? CreateNewAudioSource();
        ConfigureAudioSource(source, effect, position, dynamicVolume, loop);
        source.Play();
        // 记录播放上下文
        if (owner != null)
        {
            playbackContexts[source] = new PlaybackContext
            {
                soundName = soundName,
                owner = owner
            };
            
            // 自动清理失效引用
            StartCoroutine(MonitorOwnerLifecycle(source, owner));
        }
        return source;
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
        //清理可能存在的旧nextSource
        if (activeCycleSources.TryGetValue(source, out var oldInfo) && oldInfo.nextSource != null)
        {
            oldInfo.nextSource.Stop();
            activeCycleSources.Remove(oldInfo.nextSource);
        }
        // 初始化循环播放信息
        if (effect.useCycle && effect.cycleClips.Count > 0)
        {
            var cycleInfo = new CyclePlaybackInfo
            {
                effect = effect,
                currentIndex = 0,
                position = position,
                dynamicVolume = dynamicVolume
            };
            
            source.clip = effect.cycleClips[0];
            activeCycleSources[source] = cycleInfo;
            loop = false; // 循环列表时禁用单曲循环
        }
        else
        {
            // 没有轮播音频时从随机列表中随机选择逻辑
            source.clip = SelectRandomClip(effect);
        }
        
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
    // 处理循环列表播放
    private void HandleCyclePlayback()
    {
        float transitionDuration = 0.5f; // 过渡时间(秒)

        foreach (var kvp in activeCycleSources.ToList())
        {
            var source = kvp.Key;
            var info = kvp.Value;

            // 当前音频即将结束时(最后1秒)预加载下一个
            if (info.nextSource == null && 
                source.clip != null && 
                source.time > source.clip.length - 1f)
            {
                PrepareNextClip(source, info);
            }

            // 处理过渡中的交叉淡入淡出
            if (info.nextSource != null)
            {
                info.transitionProgress += Time.deltaTime / transitionDuration;
                
                // 当前音源淡出
                source.volume = Mathf.Lerp(
                    GetFinalVolume(source, info), 
                    0f, 
                    info.transitionProgress
                );
                
                // 下一个音源淡入
                info.nextSource.volume = Mathf.Lerp(
                    0f, 
                    GetFinalVolume(info.nextSource, info), 
                    info.transitionProgress
                );

                // 过渡完成
                if (info.transitionProgress >= 1f)
                {
                    CompleteTransition(source, info);
                }
            }
        }
    }
    // 轮播时准备下一个音源
    private void PrepareNextClip(AudioSource currentSource, CyclePlaybackInfo info)
    {
        info.currentIndex = (info.currentIndex + 1) % info.effect.cycleClips.Count;
        
        // 获取新音源
        info.nextSource = GetAvailableAudioSource() ?? CreateNewAudioSource();
        info.nextSource.clip = info.effect.cycleClips[info.currentIndex];
        
        // 配置相同参数
        info.nextSource.pitch = currentSource.pitch;
        info.nextSource.spatialBlend = currentSource.spatialBlend;
        info.nextSource.outputAudioMixerGroup = currentSource.outputAudioMixerGroup;
        info.nextSource.transform.position = currentSource.transform.position;
        
        // 初始设置
        info.nextSource.volume = 0f;
        info.nextSource.PlayDelayed(0.1f); // 提前0.1秒开始准备
        info.transitionProgress = 0f;
    }

    // 轮播完成过渡
    private void CompleteTransition(AudioSource oldSource, CyclePlaybackInfo info)
    {
        oldSource.Stop();
        activeCycleSources.Remove(oldSource);
        
        // 将新音源设为当前音源
        activeCycleSources[info.nextSource] = info;
        info.nextSource.volume = GetFinalVolume(info.nextSource, info);
        info.nextSource = null;
    }

    // 轮播计算最终音量
    private float GetFinalVolume(AudioSource source, CyclePlaybackInfo info)
    {
        float masterVolume = GetCategoryVolume(AudioCategory.Master);
        float categoryVolume = GetCategoryVolume(info.effect.category);
        return info.effect.defaultVolume * masterVolume * categoryVolume * info.dynamicVolume;
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

    // 所有者关联停止方法
    public void Stop(string soundName, MonoBehaviour owner)
    {
        foreach (var kvp in playbackContexts.ToList())
        {
            if (kvp.Value.soundName == soundName && 
                kvp.Value.owner == owner && 
                kvp.Key.isPlaying)
            {
                Stop(kvp.Key);
            }
        }
    }

    // Stop方法
    public void Stop(AudioSource source)
    {
        if (source == null) return;
        
        // 新增：停止过渡中的音源
        if (activeCycleSources.TryGetValue(source, out var info) && info.nextSource != null)
        {
            info.nextSource.Stop();
            activeCycleSources.Remove(info.nextSource); // 确保也移除
        }
        
        source.Stop();
        activeCycleSources.Remove(source);
        activeSounds.Remove(source);
        playbackContexts.Remove(source);
    }

    // 自动清理机制
    private IEnumerator MonitorOwnerLifecycle(AudioSource source, MonoBehaviour owner)
    {
        while (source.isPlaying && owner != null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        // 当所有者被销毁时自动停止音效
        if (owner == null && source.isPlaying)
        {
            Stop(source);
        }
    }
 

    public void StopAll()
    {
        foreach (var source in audioSourcePool)
        {
            source.Stop();
        }
        activeCycleSources.Clear();
        activeSounds.Clear();
        playbackContexts.Clear();
    }
    private void CleanInvalidContexts()
    {
        foreach (var kvp in playbackContexts.ToList())
        {
            if (kvp.Key == null || !kvp.Key.isPlaying)
            {
                playbackContexts.Remove(kvp.Key);
            }
        }
    }
    private void CleanInactiveSources()
    {
        foreach (var source in audioSourcePool.ToList())
        {
            if (!source.isPlaying)
            {
                activeCycleSources.Remove(source);
                activeSounds.Remove(source);
                playbackContexts.Remove(source);
            }
        }
    }
    /// <summary>
    /// 切换3D音频功能
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
    /// 控制混响效果开关
    /// </summary>
    /// <param name="enable">true启用混响，false关闭</param>
    public void ToggleReverb(bool enable)
    {
        // 通过音频混合器控制
        audioMixer.SetFloat("ReverbMix", enable ? 0f : -80f); // 0dB启用，-80dB静音
    }
    #endregion

   
}