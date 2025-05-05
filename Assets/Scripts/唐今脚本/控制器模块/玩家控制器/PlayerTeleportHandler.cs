using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Transform))]
public class PlayerTeleportHandler : MonoBehaviour
{
    [Header("位移设置")]
    [SerializeField] 
    private Transform _actualTarget; // 实际位移目标的Transform
    
    [SerializeField]
    private Transform _objectToMove; // 实际要移动和失活的对象
    
    [SerializeField]
    private bool _disableDuringTeleport = true; // 是否在位移过程中禁用对象
    
    [SerializeField]
    private float _teleportDuration = 0.5f; // 整个位移过程持续时间
    
    [Header("视觉效果")]
    [SerializeField]
    private ParticleSystem _teleportOutEffect; // 消失特效
    
    [SerializeField]
    private ParticleSystem _teleportInEffect; // 出现特效
    
    [Header("音效")]
    [SerializeField]
    private AudioClip _teleportSound; // 瞬移音效
    
    private Vector3 _positionOffset; // 位置偏移量
    
    private void Awake()
    {
        // 如果没有特别指定要移动的对象，默认使用脚本挂载的对象
        if (_objectToMove == null)
        {
            _objectToMove = this.transform;
        }
        
        if (_actualTarget == null)
        {
            Debug.LogError("实际位移目标未设置!", this);
            return;
        }
        
        CalculateOffset();
        EventBus<PlayerTeleportEvent>.Subscribe(OnTeleportRequested, this);
    }
    
    private void OnDestroy()
    {
        EventBus<PlayerTeleportEvent>.UnsubscribeAll(this);
    }
    
    public void CalculateOffset()
    {
        if (_actualTarget == null || _objectToMove == null) return;
        _positionOffset = _objectToMove.position - _actualTarget.position;
    }
    
    private void OnTeleportRequested(PlayerTeleportEvent teleportEvent)
    {
        if (_actualTarget == null || _objectToMove == null) return;
        
        StartCoroutine(TeleportProcess(teleportEvent));
    }
    
    private IEnumerator TeleportProcess(PlayerTeleportEvent teleportEvent)
    {
        if(_teleportOutEffect)
        {
            _teleportOutEffect.transform.position = _objectToMove.position;
            _teleportOutEffect.Play();
        }
        
        if(_teleportSound)
        {
            AudioSource.PlayClipAtPoint(_teleportSound, _objectToMove.position);
        }
        
        if (_disableDuringTeleport)
        {
            _objectToMove.gameObject.SetActive(false);
        }
        
        yield return new WaitForSeconds(_teleportDuration * 0.5f);
        
        var newPosition = teleportEvent.TargetPosition + _positionOffset;
        _objectToMove.position = newPosition;
        
        if (_disableDuringTeleport)
        {
            _objectToMove.gameObject.SetActive(true);
        }
        
        if(_teleportInEffect)
        {
            _teleportInEffect.transform.position = newPosition;
            _teleportInEffect.Play();
        }
        
        yield return new WaitForSeconds(_teleportDuration * 0.5f);
        
        Debug.Log($"位移完成，目标对象已移动到: {newPosition}", this);
    }
    
    [ContextMenu("重新计算偏移量")]
    private void RecalculateOffsetInEditor()
    {
        CalculateOffset();
        Debug.Log($"新的偏移量: {_positionOffset}", this);
    }
}