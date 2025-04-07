using UnityEngine;
using Obi;
using System.Collections.Generic;

public class MultiPositionEmitter : MonoBehaviour
{
    [Tooltip("List of positions to emit particles from")]
    public List<Transform> emissionPositions = new List<Transform>();

    [Tooltip("Time delay between position switches")]
    public float switchDelay = 0.5f;

    public ObiEmitter emitter;
    private bool _emissionStarted = false;
    private int _currentPositionIndex = 0;
    private float _lastSwitchTime = 0;
    private int _particlesPerPosition = 0;
    private int _particlesEmittedAtCurrentPosition = 0;
    
    private void Start()
    {
        if (emissionPositions.Count == 0)
        {
            Debug.LogWarning("No emission positions assigned. Adding current transform as default.");
            emissionPositions.Add(transform);
        }

        if (emitter != null && emitter.isLoaded)
        {
            InitializeEmission();
        }
        else
        {
            emitter.OnBlueprintLoaded += OnBlueprintLoaded;
        }
    }

    private void OnBlueprintLoaded(ObiActor actor, ObiActorBlueprint blueprint)
    {
        InitializeEmission();
        emitter.OnBlueprintLoaded -= OnBlueprintLoaded;
    }

    private void InitializeEmission()
    {
        if (emissionPositions.Count == 0) return;

        // Calculate how many particles to emit per position
        _particlesPerPosition = emitter.particleCount / emissionPositions.Count;
        
        // Move to first position
        MoveToPosition(0);
        _emissionStarted = true;
        _lastSwitchTime = Time.time;
    }

    private void Update()
    {
        if (!_emissionStarted || !emitter.isLoaded) return;

        // Check if we've emitted enough particles at current position
        if (_particlesEmittedAtCurrentPosition >= _particlesPerPosition)
        {
            // Check if we should switch to next position
            if (Time.time - _lastSwitchTime >= switchDelay)
            {
                _currentPositionIndex++;
                _particlesEmittedAtCurrentPosition = 0;
                _lastSwitchTime = Time.time;

                if (_currentPositionIndex < emissionPositions.Count)
                {
                    MoveToPosition(_currentPositionIndex);
                }
                else
                {
                    // All positions have been used, emission complete
                    _emissionStarted = false;
                    enabled = false; // Disable this script
                    return;
                }
            }
        }

        // Emit particles at current position
        if (!emitter.EmitParticle(0)) return;
        _particlesEmittedAtCurrentPosition++;
        emitter.SimulationStart(Time.deltaTime, Time.deltaTime);
    }

    private void MoveToPosition(int positionIndex)
    {
        if (positionIndex < 0 || positionIndex >= emissionPositions.Count) return;
        transform.position = emissionPositions[positionIndex].position;
        transform.rotation = emissionPositions[positionIndex].rotation;
    }

    // Editor method to add the current transform position to the list
    [ContextMenu("Add Current Position")]
    private void AddCurrentPosition()
    {
        emissionPositions ??= new List<Transform>();

        var positionObject = new GameObject("Emission Position " + (emissionPositions.Count + 1));
        positionObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
        positionObject.transform.SetParent(transform.parent);
        emissionPositions.Add(positionObject.transform);
    }
}