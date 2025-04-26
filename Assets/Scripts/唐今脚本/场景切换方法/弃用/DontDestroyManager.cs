using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroyManager : MonoBehaviour
{
    public static DontDestroyManager Instance { get; private set; }
    private readonly HashSet<GameObject> _managedObjects = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddObject(GameObject obj)
    {
        if (!obj || !_managedObjects.Add(obj)) return;

        if (obj.scene.name == "DontDestroyOnLoad") return;
        Debug.Log($"添加 {obj.name} 到 DontDestroyOnLoad", obj);
        DontDestroyOnLoad(obj);
    }

    public void RemoveObject(GameObject obj)
    {
        if (obj == null || !_managedObjects.Contains(obj)) return;

        _managedObjects.Remove(obj);
        Debug.Log($"尝试移除 {obj.name} 的 DontDestroyOnLoad 状态", obj);

        // 1. 先移除父物体（防止父物体仍在 DontDestroyOnLoad 场景）
        var originalParent = obj.transform.parent;
        obj.transform.SetParent(null);

        // 2. 尝试移动到当前场景
        SceneManager.MoveGameObjectToScene(obj, SceneManager.GetActiveScene());

        // 3. 延迟一帧后恢复父物体（确保 Unity 完全处理场景移动）
        if (originalParent != null)
        {
            StartCoroutine(DelayedReparent(obj, originalParent));
        }
    }

    private IEnumerator DelayedReparent(GameObject obj, Transform parent)
    {
        yield return null;
        if (obj && parent)
        {
            obj.transform.SetParent(parent);
        }
    }
}