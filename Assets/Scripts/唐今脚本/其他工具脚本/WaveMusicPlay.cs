using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveMusicPlay : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private float minSpeedThreshold = 5f; // Minimum speed to trigger sound
    [SerializeField] private float maxSpeedThreshold = 30f; // Speed at which volume reaches maximum
    [SerializeField] private string swingSoundName = "挥舞"; // Name of the sound to play
    [SerializeField] private float soundCooldown = 1f; // Prevent rapid consecutive sounds
    [SerializeField] private float minVolume = 0.1f; // Minimum volume at min speed
    [SerializeField] private float maxVolume = 1.0f; // Maximum volume at max speed

    private Rigidbody rb;
    private bool wasAboveThreshold = false;
    private float lastSoundTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; // Set to kinematic if you don't want physics to affect it
        }
    }

    private void FixedUpdate()
    {
        CheckSpeedForSound();
    }

    private void CheckSpeedForSound()
    {
        if (rb == null) return;
        
        float currentSpeed = rb.velocity.magnitude;
        bool canPlaySound = Time.time - lastSoundTime > soundCooldown;
        
        if (currentSpeed > minSpeedThreshold && !wasAboveThreshold && canPlaySound)
        {
            // Calculate mapped volume between min and max thresholds
            float normalizedSpeed = Mathf.InverseLerp(minSpeedThreshold, maxSpeedThreshold, currentSpeed);
            float volume = Mathf.Lerp(minVolume, maxVolume, normalizedSpeed);
            
            // Clamp the volume to ensure it's within bounds
            volume = Mathf.Clamp(volume, minVolume, maxVolume);
            
            // Play sound with speed-dependent volume
            AudioManager.Instance.Play(swingSoundName, transform.position, volume);
            
            wasAboveThreshold = true;
            lastSoundTime = Time.time;
        }
        else if (currentSpeed <= minSpeedThreshold * 0.8f) // Add some hysteresis to prevent rapid toggling
        {
            wasAboveThreshold = false;
        }
    }
}