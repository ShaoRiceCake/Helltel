using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音频配置文件（通过CreateAssetMenu创建实例）
/// </summary>
[CreateAssetMenu(fileName = "AudioConfig", menuName = "Helltel/Audio Configuration")]
public class AudioConfig : ScriptableObject
{
    [Tooltip("音效列表（所有需要使用的音效）")]
    public List<SoundEffect> soundEffects = new List<SoundEffect>();
    
    [Tooltip("初始对象池大小（根据需求调整）")]
    public int initialPoolSize = 10;
}