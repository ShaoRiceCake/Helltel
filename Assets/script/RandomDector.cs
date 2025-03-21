using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomDecor : MonoBehaviour
{
    [SerializeField]GameObject[]decorPrefabs;
    DungeonGenerator myGenerator;
    bool isCompleted;
    void Start()
    {
        myGenerator = GameObject.Find("Generator").GetComponent<DungeonGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(myGenerator != null)
        {
            if(isCompleted = false && myGenerator.dungeonState == DungeonState.completed)
            {
                isCompleted = true;
                int decorIndex = Random.Range(0, decorPrefabs.Length);
                GameObject goDecor = Instantiate(decorPrefabs[decorIndex],transform.position,Quaternion.identity,transform)as GameObject;;
                goDecor.name = decorPrefabs[decorIndex].name;
            }
        }
    }
}
