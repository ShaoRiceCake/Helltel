using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum AudioCategory
{
    Master,
    Music,
    SFX,
    Voice
}
/// <summary>
/// 音效数据结构（包含基础配置和随机化设置）
/// </summary>
[System.Serializable]
public class SoundEffect
{
    [Header("基础设置")]
    [Tooltip("音效唯一标识符（区分大小写）")]
    public string name;
    [Header("默认Clip")]
    [Tooltip("默认音频剪辑（当随机列表为空时使用）")]
    public AudioClip clip;
    
    [Range(0, 1), Tooltip("基础音量（0-1）")]
    public float defaultVolume = 1f;
    
    [Header("空间音频")]
    [Tooltip("启用3D音效效果")]
    public bool is3D = false;
    
    [Tooltip("指定音频混合组（可选）")]
    public AudioMixerGroup mixerGroup;

    [Header("启用音量/音调随机变化")]
    
    public bool useVariation = true;
    [Header("允许随机选择的音频剪辑列表，如果不放音频进去，就用默认Clip")]
    [Tooltip("允许随机选择的音频剪辑列表")]
    public List<AudioClip> randomClips = new List<AudioClip>();

    [Header("轮播列表")]
    [Tooltip("配置循环轮播的音频剪辑列表（按顺序播放）")]
    public List<AudioClip> cycleClips = new List<AudioClip>();
    
    [Tooltip("启用循环轮播功能")]
    public bool useCycle = false;

    [Header("分类设置")]
    [Tooltip("从下拉菜单中选择分类")]
    public AudioCategory category = AudioCategory.SFX;
}