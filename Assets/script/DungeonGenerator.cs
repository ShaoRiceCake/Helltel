using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;


public class DungeonGenerator : MonoBehaviour
{
    [Header("暂定为电梯")]
    public GameObject startTile;      // 起始房间预制体
    [Header("可生成的地牢房间")]
    public GameObject[] tilePrefabs;   // 可生成的地牢房间预制体集合
    [Header("可生成的墙")]
    public GameObject[] blockedPrefabs;   
    [Header("可生成的门")]
    public GameObject[] doorPrefabs;  
    [Header("终点房间")]
    public GameObject[] exitPrefabs;   

    
    Transform tileFrom, tileTo,tileRoot;       // 记录当前需要连接的两个房间（连接起点和终点）
    
    [Header("主干道长度（包含电梯）")]
    [Range(2,100)]public int mainLenght = 10;
    [Header("分支长度")]
    [Range(0,50)]public int branchLenght = 5;
    [Header("分支数")]
    [Range(0,25)]public int branchNum = 10;
    [Header("出现门的概率")]
    [Range(0,100)]public int doorPercent = 25;
    [Header("生成延迟")]
    [Range(0,1f)]public float constructionDelay;
    [Header("已生成的房间")]
    public List<Tile> generatedTiles = new List<Tile>();

    void Start()
    {
        StartCoroutine(DungeonBuild());
    }

    void Update()
    {
        
    } 
    IEnumerator DungeonBuild()
    {
        tileRoot = CreatStartTile();  // 生成初始房间
        tileTo = tileRoot;        // 生成第一个普通房间，且作为第一个要连接的房间
        ConnectTile();
        for (int i = 0; i < mainLenght -1;i++)
        {
            yield return new WaitForSeconds(constructionDelay);
            
            tileFrom = tileTo;
            tileTo = CreatTile();
            ConnectTile();
        }
    }

    // 场景重置方法
    public void UpdateDungeon()
    {
        // 显式声明加载模式
        SceneManager.LoadScene("邵智高地牢生成测试场景");
    }

    // 创建起始房间
    Transform CreatStartTile()
    {
        GameObject tile = Instantiate(startTile, Vector3.zero, Quaternion.identity);
        tile.name = "Start Room";
        float yRot = Random.Range(0, 4) * 90f;  // 随机旋转（0°、90°、180°、270°）
        tile.transform.Rotate(0, yRot, 0);       // 用于增加房间朝向的多样性

        generatedTiles.Add(new Tile(tile.transform, null));

        return tile.transform;
    }

    // 创建普通房间
    Transform CreatTile()
    {
        int tileIndex = Random.Range(0, tilePrefabs.Length);
        GameObject tile = Instantiate(tilePrefabs[tileIndex], Vector3.zero, Quaternion.identity);
        tile.name = tilePrefabs[tileIndex].name;
        float yRot = Random.Range(0, 4) * 90f;  // 同样的随机旋转逻辑
        tile.transform.Rotate(0, yRot, 0);

        Transform origin = generatedTiles[generatedTiles.FindIndex(x =>x.tile == tileFrom)].tile;
        generatedTiles.Add(new Tile(tile.transform, origin));

        return tile.transform;
    }

    // 房间连接方法
    void ConnectTile()
    {
        //获取tileFrom房间的随机未连接的连接点,作为来源连接点
        Transform connectFrom = GetRandomConnector(tileFrom);
        if (connectFrom == null) { return; }
        //获取tileTo房间的随机未连接的连接点，作为目标连接点
        Transform connectTo = GetRandomConnector(tileTo);
        if (connectTo == null) { return; }
        //将目标连接点的父物体设置为来源连接点
        connectTo.SetParent(connectFrom);
        //将目标瓦片的父物体设置为目标连接点
        tileTo.SetParent(connectTo);
        //将目标连接点本地坐标和旋转归0，并旋转在y方向180度以进行对接
        connectTo.localPosition=Vector3.zero;
        connectTo.localRotation=Quaternion.identity;
        connectTo.Rotate(0,180f,0);
        

        //对接完成后进行分离，将父子级还原,将场景的父物体设置为场景生成器
        tileTo.SetParent(transform);
        connectTo.SetParent(tileTo.Find("Connectors"));

        generatedTiles.Last().connector = connectFrom.GetComponent<Connector>();
    }

    // 获取指定房间的随机可用连接点（Connector组件）
    Transform GetRandomConnector(Transform tile)
    {
        if (tile == null) { return null; }
        // ​获取未连接的连接点的列表
        List<Connector> connectorList = tile.GetComponentsInChildren<Connector>().ToList()
            .FindAll(x => x.isConnected == false);
        
        if (connectorList.Count > 0)
        {
            int connectorIndex = Random.Range(0, connectorList.Count);
            connectorList[connectorIndex].isConnected = true;  // 标记连接点已使用
            return connectorList[connectorIndex].transform;
        }
        return null;
    }
}