using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugMeshControl : MonoBehaviour
{
    // 使用List来存储动态添加的MeshRenderer
    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

    // 添加MeshRenderer到列表中
    public void AddMeshRenderer(MeshRenderer renderer)
    {
        if (renderer != null && !meshRenderers.Contains(renderer))
        {
            meshRenderers.Add(renderer);
            Debug.Log("MeshRenderer added: " + renderer.name);
        }
    }

    // 移除MeshRenderer从列表中
    public void RemoveMeshRenderer(MeshRenderer renderer)
    {
        if (meshRenderers.Contains(renderer))
        {
            meshRenderers.Remove(renderer);
            Debug.Log("MeshRenderer removed: " + renderer.name);
        }
    }

    // 切换所有MeshRenderer的开关状态
    public void ToggleAllMeshRenderers()
    {
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = !renderer.enabled;
                Debug.Log("Toggled MeshRenderer: " + renderer.name + " - Enabled: " + renderer.enabled);
            }
        }
    }

    // 打开所有MeshRenderer
    public void EnableAllMeshRenderers()
    {
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
                Debug.Log("Enabled MeshRenderer: " + renderer.name);
            }
        }
    }

    // 关闭所有MeshRenderer
    public void DisableAllMeshRenderers()
    {
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
                Debug.Log("Disabled MeshRenderer: " + renderer.name);
            }
        }
    }
}
