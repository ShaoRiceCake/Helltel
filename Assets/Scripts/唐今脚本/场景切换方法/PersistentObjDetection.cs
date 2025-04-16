using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentObjDetection : MonoBehaviour
{
    [Tooltip("需要监测的外部触发器碰撞盒")]
    public Collider triggerCollider;

    [Tooltip("基础场景的 EcoSceneItem 父物体")]
    public Transform baseSceneParent;

    [Tooltip("只检测带有此标签的物体")]
    public string targetTag = "EcoItem"; // 默认标签，可在Inspector修改

    private readonly HashSet<Collider> _objectsInside = new();
    private bool _initialized;
    private SceneOverlayer _sceneOverlayer;

    private void Start()
    {
        if (triggerCollider == null || baseSceneParent == null)
        {
            Debug.LogError("Trigger collider or baseSceneParent is not assigned!", this);
            enabled = false;
            return;
        }

        if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning("The assigned collider is not set as a trigger. It's recommended to set it as trigger for proper functionality.", this);
        }

        _sceneOverlayer = FindObjectOfType<SceneOverlayer>();
        if (_sceneOverlayer == null)
        {
            Debug.LogError("SceneOverlayer not found in the scene!", this);
            enabled = false;
            return;
        }

        StartCoroutine(DelayedInitialDetection());
    }

    private System.Collections.IEnumerator DelayedInitialDetection()
    {
        yield return null;
        PerformInitialDetection();
        _initialized = true;
    }

    private void PerformInitialDetection()
    {
        if (!triggerCollider) return;

        _objectsInside.Clear();

        var colliders = Physics.OverlapBox(
            triggerCollider.bounds.center,
            triggerCollider.bounds.extents,
            triggerCollider.transform.rotation
        );

        foreach (var col in colliders)
        {
            if (col == triggerCollider || !col.enabled || !col.gameObject.activeInHierarchy) continue;
            if (!col.CompareTag(targetTag)) continue; 

            _objectsInside.Add(col);
            SetParentToBaseScene(col.transform);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_initialized || other == triggerCollider || !other.CompareTag(targetTag)) return;

        if (!_objectsInside.Add(other)) return;
        SetParentToBaseScene(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_initialized || other == triggerCollider || !other.CompareTag(targetTag)) return;

        if (!_objectsInside.Remove(other)) return;
        SetParentToActiveAdditiveScene(other.transform);
    }

    private void SetParentToBaseScene(Transform objTransform)
    {
        if (!objTransform) return;
        objTransform.SetParent(baseSceneParent, true);
    }

    private void SetParentToActiveAdditiveScene(Transform objTransform)
    {
        if (!objTransform) return;
        var targetParent = GetActiveAdditiveSceneParent();
        objTransform.SetParent(targetParent, true);
    }

    private Transform GetActiveAdditiveSceneParent()
    {
        if (_sceneOverlayer == null) return baseSceneParent;

        var activeSceneName = _sceneOverlayer.GetCurrentLoadedSceneName();
        if (string.IsNullOrEmpty(activeSceneName)) return baseSceneParent;

        var additiveScene = SceneManager.GetSceneByName(activeSceneName);
        if (!additiveScene.IsValid() || !additiveScene.isLoaded) return baseSceneParent;

        var rootObjects = additiveScene.GetRootGameObjects();
        foreach (var rootObj in rootObjects)
        {
            var ecoSceneItem = FindEcoSceneItem(rootObj.transform);
            if (ecoSceneItem) return ecoSceneItem;
        }

        return baseSceneParent;
    }

    private static Transform FindEcoSceneItem(Transform parent)
    {
        return parent.name == "EcoSceneItem" ? parent : (from Transform child in parent select FindEcoSceneItem(child)).FirstOrDefault(result => result);
    }
}