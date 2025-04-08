using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshController : MonoBehaviour
{
    // Start is called before the first frame update
    private NavMeshSurface navMeshSurface;
    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            updateNavMesh();
        }
    }
    private void updateNavMesh()
    {
        navMeshSurface.RemoveData();
        navMeshSurface.BuildNavMesh();
    }

}
