using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 单条音效数据结构（可序列化）
/// </summary>
[System.Serializable]
public class SoundEffect
{
    [Tooltip("音效标识名称（需唯一）")]
    public string name;
    
    [Tooltip("音频文件引用")]
    public AudioClip clip;
    
    [Tooltip("默认音量（0-1）")]
    [Range(0, 1)] 
    public float defaultVolume = 1f;
    
    [Tooltip("是否启用3D音效")]
    public bool is3D = true;
    
    [Tooltip("指定音频混合组（可选）")]
    public AudioMixerGroup mixerGroup;

    [Header("随机化设置")]
    [Tooltip("是否启用音调/音量随机变化")]
    public bool useVariation = true;
    
    [Tooltip("允许随机选择的音频剪辑列表（留空则使用单个clip）")]
    public List<AudioClip> randomClips = new List<AudioClip>();
}