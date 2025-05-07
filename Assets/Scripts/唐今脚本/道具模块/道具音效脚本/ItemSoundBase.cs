using UnityEngine;

/// <summary>
/// 道具音效基类
/// </summary>
public abstract class ItemSoundBase : MonoBehaviour
{
    [Header("音效设置")]
    [Tooltip("音效资源名称")]
    [SerializeField] protected string soundName = "ItemCollision";
    
    [Tooltip("音效音量")]
    [SerializeField] protected float volume = 1f;
    
    [Tooltip("最低播放间隔(秒)，避免短时间内重复播放")]
    [SerializeField] protected float minPlayInterval = 0.2f;
    
    [Header("碰撞检测设置")]
    [Tooltip("排除检测的层级，与这些层碰撞不会播放音效")]
    [SerializeField] protected LayerMask excludeLayers;
    
    // 上次播放时间记录
    private float _lastPlayTime;

    protected virtual void Start()
    {
        var playerLayer = LayerMask.NameToLayer("Player");
        if(playerLayer != -1)
        {
            excludeLayers |= (1 << playerLayer); 
        }
    }
    
    /// <summary>
    /// 碰撞进入时播放音效
    /// </summary>
    /// <param name="other">碰撞信息</param>
    protected virtual void OnCollisionEnter(Collision other)
    {
        // 检查是否在排除层
        if (excludeLayers == (excludeLayers | (1 << other.gameObject.layer)))
        {
            return;
        }
        
        // 检查播放间隔
        if (Time.time - _lastPlayTime < minPlayInterval)
        {
            return;
        }
        
        // 播放音效
        PlaySound(other.contacts.Length > 0 ? other.contacts[0].point : transform.position);
    }
    
    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="position">音效播放位置</param>
    protected virtual void PlaySound(Vector3 position)
    {
        if (AudioManager.Instance == null || string.IsNullOrEmpty(soundName)) return;
        AudioManager.Instance.Play(soundName, position, volume);
        _lastPlayTime = Time.time;
    }
}