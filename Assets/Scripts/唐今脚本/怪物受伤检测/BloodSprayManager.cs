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

        // 直接在当前游戏对象下创建喷射器
        CreateEmitter(this.transform, sprayEvent);
    }

    private void CreateEmitter(Transform parent, BloodSprayEvent sprayEvent)
    {
        // 实例化喷射器GameObject作为当前对象的子对象
        var emitterObj = Instantiate(
            bloodSprayPrefab,
            sprayEvent.spawnPosition,  // 使用事件中的位置
            sprayEvent.spawnRotation,  // 使用事件中的旋转
            parent  // 父对象设为当前脚本挂载的对象
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

        // 将喷射器对象添加到活动列表
        _activeSprays.Add(emitterObj);
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