using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BloodSprayManager : MonoBehaviour
{
    [Header("喷射器设置")]
    [Tooltip("血液喷射器预制体（包含CustomEmitterController的GameObject）")]
    public GameObject bloodSprayPrefab;

    private readonly List<GameObject> _activeSprays = new List<GameObject>();

    private void OnEnable()
    {
        EventBus<BloodSprayEvent>.Subscribe(OnBloodSpray, this);
    }

    private void OnDisable()
    {
        EventBus<BloodSprayEvent>.UnsubscribeAll(this);
    }

    private void OnBloodSpray(BloodSprayEvent sprayEvent)
    {
        if (bloodSprayPrefab == null)
        {
            Debug.LogError("血液喷射器预制体未设置！");
            return;
        }

        // 创建容器对象管理一组喷射器
        var container = new GameObject("BloodSprayGroup")
        {
            transform =
            {
                position = sprayEvent.spawnPosition,
                rotation = sprayEvent.spawnRotation
            }
        };


        CreateEmitter(container.transform, sprayEvent);


   
        _activeSprays.Add(container);
    }

    private void CreateEmitter(Transform parent, BloodSprayEvent sprayEvent)
    {
        // 实例化喷射器GameObject
        var emitterObj = Instantiate(
            bloodSprayPrefab,
            parent.position,
            parent.rotation * Quaternion.Euler(0, 180, 0), // 调整局部-x方向
            parent
        );

        // 获取喷射器控制器脚本
        var emitter = emitterObj.GetComponentInChildren<CustomEmitterController>();
        if (emitter == null)
        {
            Debug.LogError("喷射器预制体上未找到CustomEmitterController组件！");
            Destroy(emitterObj);
            return;
        }

        // 设置参数并执行发射
        emitter.SetEmissionSpeed(sprayEvent.emissionSpeed);
        emitter.SetEmissionRandomness(sprayEvent.emissionRandomness);
        emitter.Emit();
    }

    public void ClearAllSprays()
    {
        foreach (var spray in _activeSprays.Where(spray => spray != null))
        {
            Destroy(spray);
        }

        _activeSprays.Clear();
    }
}