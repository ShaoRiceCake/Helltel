using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemWeapon_BaseballBat : PassiveItem
{
    [Header("Baseball Bat Settings")]
    [SerializeField] private GameObject associatedObject; // Reference to the external object
    
    [Header("Swing Sound Settings")]
    [SerializeField] private float velocityThreshold = 5f; // Minimum speed to play sound
    [SerializeField] private float soundCooldown = 0.5f; // Time between sound plays
    [SerializeField] private float soundVolume = 1.0f; // Volume of the swing sound

    private Rigidbody _rb;
    private float _lastSoundTime;
    private bool _soundReady = true;

    private bool _onCatch;
    
    protected override void Awake()
    {
        base.Awake(); 
        _rb = GetComponent<Rigidbody>();
        
        if (_rb != null) return;
        _rb = gameObject.AddComponent<Rigidbody>();
        _rb.isKinematic = true;

    }

    private void Start()
    {
        OnGrabbed.AddListener(OnBaseballBatGrabbed);
        OnReleased.AddListener(OnBaseballBatReleased);
    }

    private void OnBaseballBatReleased()
    {
        _onCatch = false;
    }

    private void OnBaseballBatGrabbed()
    {
        _onCatch = true;
    }


    private void FixedUpdate()
    {
        UpdateAssociatedObjectLayer();
        CheckSwingVelocity();
        Debug.Log("_onCatch" +_onCatch);
    }

    private void UpdateAssociatedObjectLayer()
    {
        if (associatedObject)
        {
            associatedObject.layer = gameObject.layer;
        }
    }

    private void CheckSwingVelocity()
    {
        if(!_onCatch) return;
        if (_rb == null || !_soundReady) return;
        if (!(_rb.velocity.magnitude > velocityThreshold)) return;
        PlaySwingSound();
        StartCoroutine(SoundCooldown());
    }

    private void PlaySwingSound()
    {
        AudioManager.Instance.Play("挥舞", transform.position, soundVolume);
    }

    private IEnumerator SoundCooldown()
    {
        _soundReady = false;
        yield return new WaitForSeconds(soundCooldown);
        _soundReady = true;
    }
}