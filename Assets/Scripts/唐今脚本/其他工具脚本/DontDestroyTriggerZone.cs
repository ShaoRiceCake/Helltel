using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class DontDestroyTriggerZone : MonoBehaviour
{
    public Collider triggerCollider;
    private HashSet<GameObject> objectsInTrigger = new HashSet<GameObject>();

    private void Awake()
    {
        if (triggerCollider == null) triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    private void Start()
    {
        // 延迟一帧检测初始物体
        StartCoroutine(DelayedInitialCheck());
    }

    private IEnumerator DelayedInitialCheck()
    {
        yield return null; // 等待一帧让所有物体初始化完成
        DetectInitialObjects();
    }

    private void DetectInitialObjects()
    {
        if (triggerCollider == null) return;

        Collider[] colliders = Physics.OverlapBox(
            triggerCollider.bounds.center,
            triggerCollider.bounds.extents,
            triggerCollider.transform.rotation);

        foreach (Collider col in colliders)
        {
            if (col.gameObject != this.gameObject && !objectsInTrigger.Contains(col.gameObject))
            {
                objectsInTrigger.Add(col.gameObject);
                DontDestroyObject(col.gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != this.gameObject && !objectsInTrigger.Contains(other.gameObject))
        {
            objectsInTrigger.Add(other.gameObject);
            DontDestroyObject(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject != this.gameObject && objectsInTrigger.Contains(other.gameObject))
        {
            objectsInTrigger.Remove(other.gameObject);
            AllowDestroyObject(other.gameObject);
        }
    }

    private void DontDestroyObject(GameObject obj)
    {
        if (obj.scene.name != "DontDestroyOnLoad")
        {
            Debug.Log($"设置 {obj.name} 为 DontDestroyOnLoad", obj);
            DontDestroyOnLoad(obj);
        }
    }

    private void AllowDestroyObject(GameObject obj)
    {
        Debug.Log($"移除 {obj.name} 的 DontDestroyOnLoad 状态", obj);
        
        // 方法1：临时解除父子关系
        Transform originalParent = obj.transform.parent;
        obj.transform.SetParent(null);
        
        // 移动到当前活动场景
        SceneManager.MoveGameObjectToScene(obj, SceneManager.GetActiveScene());
        
        // 恢复父子关系
        if (originalParent != null)
        {
            obj.transform.SetParent(originalParent);
        }
        
        // 方法2：替代方案 - 创建新实例并销毁原对象
        // GameObject newObj = Instantiate(obj);
        // newObj.transform.SetPositionAndRotation(obj.transform.position, obj.transform.rotation);
        // if (obj.transform.parent != null)
        // {
        //     newObj.transform.SetParent(obj.transform.parent);
        // }
        // Destroy(obj);
    }
    
}