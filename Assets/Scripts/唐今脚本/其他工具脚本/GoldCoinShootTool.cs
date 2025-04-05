using System.Collections;
using UnityEngine;

public class GoldCoinShootTool : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform spawnPoint;          // Spawn position
    public GameObject spawnPrefab;        // Prefab to spawn
    public float moveSpeed = 5f;          // Movement speed of spawned objects
    public float lifetime = 3f;           // How long before destroying spawned objects
    public float rotationSpeed = 180f;    // Rotation speed in degrees per second

    private FluidParticleCounter _particleCounter;

    private void Awake()
    {
        _particleCounter = GetComponent<FluidParticleCounter>();
        if (_particleCounter == null)
        {
            Debug.LogError("FluidParticleCounter component not found!");
            return;
        }

        _particleCounter.OnParticleDestroyed += OnParticleDestroyed;
    }

    private void OnDestroy()
    {
        if (_particleCounter != null)
        {
            _particleCounter.OnParticleDestroyed -= OnParticleDestroyed;
        }
    }

    private void OnParticleDestroyed(int particleIndex)
    {
        StartCoroutine(SpawnAndMoveObject());
    }

    private IEnumerator SpawnAndMoveObject()
    {
        if (!spawnPrefab || !spawnPoint) yield break;

        var newObj = Instantiate(spawnPrefab, spawnPoint.position, Quaternion.identity);
        
        var timer = 0f;
        while (timer < lifetime && newObj)
        {
            // Move upward
            newObj.transform.Translate(Vector3.up * (moveSpeed * Time.deltaTime));
            
            // Rotate around Y-axis (or any axis you prefer)
            newObj.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            
            timer += Time.deltaTime;
            yield return null;
        }

        if (newObj)
        {
            Destroy(newObj);
        }
    }
}