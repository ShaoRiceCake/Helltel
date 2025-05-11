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

    private Rigidbody _rigidbody;
    private float _lastSoundTime;
    private bool _soundReady = true;

    private bool _onCatch;
    
    protected override void Awake()
    {
        base.Awake(); 
        _rigidbody = GetComponent<Rigidbody>();
        
        if (_rigidbody != null) return;
        _rigidbody = gameObject.AddComponent<Rigidbody>();
        _rigidbody.isKinematic = true;

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
        if (!_rigidbody || !_soundReady) return;
        if (!(_rigidbody.velocity.magnitude > velocityThreshold)) return;
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