using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 音频系统核心管理类（单例模式）
/// 功能包括：
/// - 音效对象池管理
/// - 全局音量控制
/// - 3D/2D音效播放
/// - 音频淡入淡出
/// - 批量控制功能
/// </summary>
public class AudioManager : MonoBehaviour
{
    // 单例实例（私有设置，全局访问）
    public static AudioManager Instance { get; private set; }

    [Header("基本配置")]
    [Tooltip("音频配置文件引用")]
    [SerializeField] private AudioConfig config;
    
    [Tooltip("默认音频混合组（当音效未指定时使用）")]
    [SerializeField] private AudioMixerGroup defaultMixerGroup;

    // 运行时数据结构
    private Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>(); // 音效查找字典
    private List<AudioSource> audioSourcePool = new List<AudioSource>(); // AudioSource对象池
    private float globalVolume = 1f; // 全局音量系数（0-1）

    /// <summary>
    /// 初始化单例和音频系统
    /// </summary>
    private void Awake()
    {
        // 单例模式初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 跨场景保持

        InitializeAudioPool();  // 初始化对象池
        LoadAudioConfig();     // 加载音频配置
    }

    /// <summary>
    /// 初始化音频对象池
    /// 根据配置创建初始数量的AudioSource组件
    /// </summary>
    private void InitializeAudioPool()
    {
        for (int i = 0; i < config.initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }
    }

    /// <summary>
    /// 创建新的AudioSource并加入对象池
    /// </summary>
    /// <returns>新创建的AudioSource</returns>
    private AudioSource CreateNewAudioSource()
    {
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false; // 禁止自动播放
        audioSourcePool.Add(newSource);
        return newSource;
    }

    /// <summary>
    /// 加载音频配置到字典
    /// 将ScriptableObject中的配置转换为快速查找的字典结构
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
            soundEffects[effect.name] = effect;
        }
    }

    /// <summary>
    /// 播放指定音效
    /// </summary>
    /// <param name="soundName">音效名称</param>
    /// <param name="position">播放位置（3D音效有效）</param>
    /// <param name="volumeMultiplier">音量系数（叠加在默认音量和全局音量上）</param>
    /// <param name="loop">是否循环播放</param>
    /// <returns>用于控制的AudioSource实例</returns>
    // 在原有方法基础上修改Play方法
    public AudioSource Play(string soundName, 
                          Vector3 position = default, 
                          float volumeMultiplier = 1f, 
                          bool loop = false,
                          bool isCritical = false) // 新增关键音效标识
    {
        if (!soundEffects.TryGetValue(soundName, out SoundEffect effect))
        {
            Debug.LogError($"音效不存在: {soundName}");
            return null;
        }

        AudioSource source = GetAvailableAudioSource() ?? CreateNewAudioSource();
        
        //随机选择音频剪辑（优先使用randomClips）
        AudioClip selectedClip = effect.clip;
        if (effect.randomClips.Count > 0)
        {
            int randomIndex = Random.Range(0, effect.randomClips.Count);
            selectedClip = effect.randomClips[randomIndex];
        }

        // 配置参数时增加随机化处理
        SetupAudioSource(source, effect, position, volumeMultiplier, loop, selectedClip);
        source.Play();
        return source;
    }

    /// <summary>
    /// 从对象池中获取闲置的AudioSource
    /// </summary>
    private AudioSource GetAvailableAudioSource()
    {
        foreach (var source in audioSourcePool)
        {
            if (!source.isPlaying) return source;
        }
        return null; // 所有源都在使用
    }

    /// <summary>
    /// 配置AudioSource参数
    /// </summary>
    private void SetupAudioSource(AudioSource source, SoundEffect effect, 
                                 Vector3 position, float volumeMultiplier, bool loop,AudioClip clip)
    {
        // 基础设置
        source.clip = clip;
        float baseVolume = effect.defaultVolume;
        //应用随机变化
        if (effect.useVariation )
        {
            // 音量随机：±5% 变化
            float volumeVariation = Random.Range(-0.1f, 0.1f);
            source.volume = Mathf.Clamp(baseVolume + volumeVariation, 0f, 1f);

            // 音调随机：±5% 变化（保持合理范围）
            float pitchVariation = Random.Range(-0.1f, 0.1f);
            source.pitch = Mathf.Clamp(1.0f + pitchVariation, 0.5f, 2.0f);
        }
        else
        {
            source.volume = baseVolume;
            source.pitch = 1.0f; // 保持原始音调
        }
        source.volume = baseVolume * globalVolume * volumeMultiplier; // 三级音量控制
        source.loop = loop;
        
        // 空间音频设置
        source.spatialBlend = effect.is3D ? 1f : 0f; // 0=2D, 1=3D
        source.outputAudioMixerGroup = effect.mixerGroup ?? defaultMixerGroup;

        // 3D音效定位
        if (effect.is3D)
        {
            source.transform.position = position;
            source.rolloffMode = AudioRolloffMode.Logarithmic; // 使用对数衰减
        }
    }

    /// <summary>
    /// 设置全局音量（影响所有音效）
    /// </summary>
    /// <param name="volume">0-1之间的音量系数</param>
    public void SetGlobalVolume(float volume)
    {
        globalVolume = Mathf.Clamp01(volume);
        
        // 实时更新所有正在播放的音效
        foreach (var source in audioSourcePool)
        {
            if (source.isPlaying)
            {
                string soundName = GetSoundName(source);
                if (soundEffects.TryGetValue(soundName, out SoundEffect effect))
                {
                    source.volume = effect.defaultVolume * globalVolume;
                }
            }
        }
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
    /// 暂停所有音效播放
    /// </summary>
    public void PauseAll()
    {
        foreach (var source in audioSourcePool)
        {
            source.Pause();
        }
    }

    /// <summary>
    /// 恢复所有暂停的音效
    /// </summary>
    public void ResumeAll()
    {
        foreach (var source in audioSourcePool)
        {
            source.UnPause();
        }
    }

    /// <summary>
    /// 淡出音效
    /// </summary>
    /// <param name="source">目标音源</param>
    /// <param name="duration">淡出时间（秒）</param>
    public void FadeOut(AudioSource source, float duration)
    {
        StartCoroutine(FadeRoutine(source, 0, duration));
    }

    /// <summary>
    /// 淡入音效
    /// </summary>
    public void FadeIn(AudioSource source, float duration)
    {
        StartCoroutine(FadeRoutine(source, 
            soundEffects[GetSoundName(source)].defaultVolume * globalVolume, 
            duration));
    }

    /// <summary>
    /// 音量渐变协程
    /// </summary>
    private IEnumerator FadeRoutine(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float timer = 0;

        while (timer < duration)
        {
            // 线性插值计算音量
            source.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        source.volume = targetVolume;
        
        // 淡出完成后停止播放
        if (Mathf.Approximately(targetVolume, 0)) 
        {
            source.Stop();
        }
    }

    /// <summary>
    /// 通过AudioSource反向查找音效名称
    /// （注意：此方法效率较低，建议仅在必要时使用）
    /// </summary>
    private string GetSoundName(AudioSource source)
    {
        foreach (var pair in soundEffects)
        {
            if (pair.Value.clip == source.clip)
            {
                return pair.Key;
            }
        }
        return "Unknown";
    }
}