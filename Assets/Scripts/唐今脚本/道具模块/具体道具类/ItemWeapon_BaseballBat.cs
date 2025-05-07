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

    private Rigidbody rb;
    private float lastSoundTime;
    private bool soundReady = true;

    protected override void Awake()
    {
        base.Awake(); // Preserve any existing Awake functionality
        rb = GetComponent<Rigidbody>();
        
        // Add Rigidbody if missing (configured as kinematic by default)
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
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
        if (rb == null || !soundReady) return;

        // Check if velocity exceeds threshold
        if (rb.velocity.magnitude > velocityThreshold)
        {
            PlaySwingSound();
            StartCoroutine(SoundCooldown());
        }
    }

    private void PlaySwingSound()
    {
        AudioManager.Instance.Play("挥舞", transform.position, soundVolume);
    }

    private IEnumerator SoundCooldown()
    {
        soundReady = false;
        yield return new WaitForSeconds(soundCooldown);
        soundReady = true;
    }
}