using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GoldCoinShootTool : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform spawnPoint;         
    public GameObject spawnPrefab;       
    public float moveSpeed = 5f;       
    public float lifetime = 3f;           
    public float rotationSpeed = 180f;    
    private FluidParticleCounter _particleCounter;
    private ObjectPool<GameObject> _objectPool;

    private void Awake()
    {
        _particleCounter = GetComponent<FluidParticleCounter>();
        if (_particleCounter == null)
        {
            Debug.LogError("FluidParticleCounter component not found!");
            return;
        }

        _objectPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(spawnPrefab),
            actionOnGet: (obj) => {
                obj.transform.position = spawnPoint.position;
                obj.transform.rotation = Quaternion.identity;
                obj.SetActive(true);
            },
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: Destroy,
            collectionCheck: true,
            defaultCapacity: 100,
            maxSize: 500
        );

        _particleCounter.OnParticleDestroyed += OnParticleDestroyed;
    }

    private void OnDestroy()
    {
        if (_particleCounter != null)
        {
            _particleCounter.OnParticleDestroyed -= OnParticleDestroyed;
        }
        
        _objectPool?.Dispose();
    }

    private void OnParticleDestroyed(int particleIndex)
    {
        StartCoroutine(SpawnAndMoveObject());
    }

    private IEnumerator<GameObject> SpawnAndMoveObject()
    {
        if (!spawnPrefab || !spawnPoint) yield break;

        var pooledObj = _objectPool.Get();
        
        var timer = 0f;
        while (timer < lifetime && pooledObj && pooledObj.activeInHierarchy)
        {
            pooledObj.transform.Translate(Vector3.up * (moveSpeed * Time.deltaTime));
            
            pooledObj.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            
            timer += Time.deltaTime;
            yield return null;
        }

        if (pooledObj && pooledObj.activeInHierarchy)
        {
            _objectPool.Release(pooledObj);
        }
    }
}