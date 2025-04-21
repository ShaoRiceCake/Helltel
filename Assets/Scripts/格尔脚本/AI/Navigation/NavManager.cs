using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class NavManager : MonoBehaviour
{
    // Start is called before the first frame update

    NavMeshSurface[] _navMeshSurfaces;
    void Awake()
    {
        _navMeshSurfaces = GetComponents<NavMeshSurface>(); 
    }

    void Start()
    {
        NavMeshUpdate();
        
    }
    // Update is called once per frame
    void NavMeshUpdate()
    {
        Debug.Log("NavMesh Update Called");
        foreach(var surface in _navMeshSurfaces)
        {
            if(surface.navMeshData != null)
            {
                surface.UpdateNavMesh(surface.navMeshData);
            }
            else
            {
                surface.BuildNavMesh();
            }
        }
        Debug.Log("NavMesh Update Complete!, surface count: " + _navMeshSurfaces.Length);
    }
}
