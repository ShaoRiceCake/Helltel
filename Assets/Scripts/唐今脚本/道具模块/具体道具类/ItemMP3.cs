using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMP3 : ActiveItem
{
    private bool _isPlaying;
    protected override void Awake()
    {
        base.Awake();
        OnUseStart.AddListener(StartUseProcess);
        OnReleased.AddListener(ReleaseProcess);
    }
    
    private void StartUseProcess()
    {
        if (_isPlaying) return;
        
        _isPlaying = true;
        AudioManager.Instance.Play("MP3音乐",transform.position, owner :this);
    }

    private void ReleaseProcess()
    {
        _isPlaying = false;
        AudioManager.Instance.Stop("MP3音乐",owner :this);
    }
    private void OnDestroy()
    {
        OnUseStart.RemoveListener(StartUseProcess);
    }
}
