using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugMeshControl : MonoBehaviour
{
    // ʹ��List���洢��̬��ӵ�MeshRenderer
    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

    // ���MeshRenderer���б���
    public void AddMeshRenderer(MeshRenderer renderer)
    {
        if (renderer != null && !meshRenderers.Contains(renderer))
        {
            meshRenderers.Add(renderer);
            Debug.Log("MeshRenderer added: " + renderer.name);
        }
    }

    // �Ƴ�MeshRenderer���б���
    public void RemoveMeshRenderer(MeshRenderer renderer)
    {
        if (meshRenderers.Contains(renderer))
        {
            meshRenderers.Remove(renderer);
            Debug.Log("MeshRenderer removed: " + renderer.name);
        }
    }

    // �л�����MeshRenderer�Ŀ���״̬
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

    // ������MeshRenderer
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

    // �ر�����MeshRenderer
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
