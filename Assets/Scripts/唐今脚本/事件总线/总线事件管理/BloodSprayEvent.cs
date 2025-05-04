using UnityEngine;

public class BloodSprayEvent
{
    public Vector3 spawnPosition;
    public Quaternion spawnRotation;
    public int emitterCount;
    public float emissionSpeed;
    public float emissionRandomness;

    public BloodSprayEvent(Vector3 position, Quaternion rotation, int count, float speed, float randomness)
    {
        spawnPosition = position;
        spawnRotation = rotation;
        emitterCount = count;
        emissionSpeed = speed;
        emissionRandomness = randomness;
    }
}