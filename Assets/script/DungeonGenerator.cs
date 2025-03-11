using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using  System.Linq;
using Unity.Collections;
public class DungeonGenerator : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public GameObject startTile;
    Transform tileFrom,tileTo;
    void Start()
    {
        tileFrom =  CreatStartTile();
        tileTo =  CreatTile();
    }

    void Update()
    {
        
    }
    void UpdateDungeon()
    {
        SceneManager.LoadScene("邵智高地牢生成测试场景");
    }
    Transform CreatStartTile()
    {
        GameObject tile = Instantiate(startTile,Vector3.zero, Quaternion.identity) as GameObject;
        tile.name = "Start Room";
        float yRot = Random.Range(0, 4)*90f;
        tile.transform.Rotate(0,yRot,0);
        return tile.transform;
    }
    Transform CreatTile()
    {
        int tileIndex = Random.Range(0, tilePrefabs.Length);
        GameObject tile = Instantiate(tilePrefabs[tileIndex],Vector3.zero, Quaternion.identity) as GameObject;
        tile.name = tilePrefabs[tileIndex].name;
        float yRot = Random.Range(0, 4)*90f;
        tile.transform.Rotate(0,yRot,0);
        return tile.transform;
    }
    void ConnectTile()
    {
        Transform connectFrom = GetRandomConnector(tileFrom);
        if (connectFrom == null){return;}
        Transform connectTo = GetRandomConnector(tileTo);
        if (connectTo == null){return;}
    }
    Transform GetRandomConnector(Transform tile)
    {
        if (tile == null){return null;}
        List<Connector> connectorList = tile.GetComponentsInChildren<Connector>().ToList().FindAll(x => x.isConnected = false);
        if (connectorList.Count > 0)
        {
            int connectorIndex = Random.Range(0, connectorList.Count);
            connectorList[connectorIndex].isConnected = true;
            return connectorList[connectorIndex].transform;
        }
        return null;
    }
}
