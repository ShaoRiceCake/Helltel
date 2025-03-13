using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using System.ComponentModel;
using Unity.Collections;

/// <summary>
/// 地牢生成器核心类
/// 实现基于连接点的程序化地牢生成系统
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    [Header("暂定为电梯")]
    public GameObject startTile;      // 起始房间预制体（通常是入口或电梯）
    [Header("可生成的地牢房间")]
    public GameObject[] tilePrefabs;   // 普通房间预制体集合（用于生成随机房间）
    [Header("可生成的墙")]
    public GameObject[] blockedPrefabs;   // 障碍物/墙壁预制体集合（未在代码中使用，预留接口） 
    [Header("可生成的门")]
    public GameObject[] doorPrefabs;      // 门预制体集合（未在代码中使用，预留接口）
    [Header("终点房间")]
    public GameObject[] exitPrefabs;       // 出口房间预制体集合（未在代码中使用，预留接口）

    // 房间连接相关变量
    Transform tileFrom, tileTo;        // 当前需要连接的两个房间（from为前一个房间，to为新房间）
    Transform tileRoot;                // 起始房间的根节点
    Transform container;

    [Header("生成参数")]
    [Header("主干道长度（包含电梯）")]
    [Range(2,100)]public int mainLenght = 10;  // 主路径总长度（包含起始房间）
    [Header("分支长度")]
    [Range(0,50)]public int branchLenght = 5;  // 每个分支的最大长度（当前未实现）
    [Header("分支数")]
    [Range(0,25)]public int branchNum = 10;    // 分支数量（当前未实现）
    [Header("出现门的概率")]
    [Range(0,100)]public int doorPercent = 25; // 门生成概率（当前未实现）
    [Header("生成延迟")]
    [Range(0,1f)]public float constructionDelay; // 房间生成间隔时间（视觉效果）
    [Header("已生成的房间"),SerializeField]
    private List<Tile> generatedTiles = new List<Tile>(); // 所有已生成房间的记录
    [Header("可用的连接口"),SerializeField]
    private List<Connector> availableConnectors = new List<Connector>();

    void Start()
    {
        // 启动地牢生成协程
        StartCoroutine(DungeonBuild());
    }

    /// <summary>
    /// 地牢生成协程（核心生成逻辑）
    /// 按顺序生成主路径房间并进行连接
    /// </summary>
    IEnumerator DungeonBuild()
    {
        GameObject goContainer = new GameObject("Main Path");
        container = goContainer.transform;
        container.SetParent(transform);
        
        // 生成起始房间并初始化连接关系
        tileRoot = CreatStartTile();  
        tileTo = tileRoot;        // 初始化第一个待连接房间
        
        // 生成主路径
        for (int i = 0; i < mainLenght -1; i++) // -1因为起始房间已创建
        {
            ConnectTiles(); // 连接当前房间与下一个房间
            
            yield return new WaitForSeconds(constructionDelay); // 延迟生成效果
            
            // 更新连接关系
            tileFrom = tileTo;
            tileTo = CreatTile(); // 创建新的普通房间
        }

        foreach(Connector connector in container.GetComponentsInChildren<Connector>())
        {
            if (connector.isConnected == false)
            {
                if(!availableConnectors.Contains(connector)){
                    availableConnectors.Add(connector);
                }
            }
        }

        for (int b = 0;b<branchNum;b++)
        {
            if(availableConnectors.Count > 0)
            {
                goContainer = new GameObject("Branch"+(b+1));
                container = goContainer.transform;
                container.SetParent(transform);

                int availableConnectorIndex = Random.Range(0,availableConnectors.Count);
                tileRoot = availableConnectors[availableConnectorIndex].transform.parent.parent;
                availableConnectors.RemoveAt(availableConnectorIndex);
                tileTo = tileRoot;
                for (int i = 0;i<branchLenght-1;i++)
                {
                    yield return new WaitForSeconds(constructionDelay);
                    tileFrom = tileTo;
                    tileTo = CreatTile();
                    ConnectTiles();
                }
            }
            else
            {break;}
        }
    }

    /// <summary>
    /// 场景重置方法（重新加载当前场景）
    /// </summary>
    public void UpdateDungeon()
    {
        SceneManager.LoadScene("邵智高地牢生成测试场景");
    }

    /// <summary>
    /// 创建起始房间
    /// </summary>
    Transform CreatStartTile()
    {
        // 实例化并初始化起始房间
        GameObject tile = Instantiate(startTile, Vector3.zero, Quaternion.identity,container);
        tile.name = "Start Room";
        
        // 设置随机旋转（增加多样性）
        float yRot = Random.Range(0, 4) * 90f;  
        tile.transform.Rotate(0, yRot, 0);       

        // 记录到已生成房间列表
        generatedTiles.Add(new Tile(tile.transform, null));

        return tile.transform;
    }

    /// <summary>
    /// 创建普通房间
    /// </summary>
    Transform CreatTile()
    {
        // 随机选择房间预制体
        int tileIndex = Random.Range(0, tilePrefabs.Length);
        GameObject tile = Instantiate(tilePrefabs[tileIndex], Vector3.zero, Quaternion.identity,container);
        tile.name = tilePrefabs[tileIndex].name;
        
        // 设置随机旋转
        float yRot = Random.Range(0, 4) * 90f;  
        tile.transform.Rotate(0, yRot, 0);

        // 记录父房间并添加到列表
        Transform origin = generatedTiles[generatedTiles.FindIndex(x =>x.tile == tileFrom)].tile;
        generatedTiles.Add(new Tile(tile.transform, origin));

        return tile.transform;
    }

    /// <summary>
    /// 连接两个房间的核心方法
    /// 通过连接点（Connector）进行物理连接
    /// </summary>
    void ConnectTiles()
    {
        // 获取双方可用的连接点
        Transform connectFrom = GetRandomConnector(tileFrom);
        if (connectFrom == null) { return; }
        Transform connectTo = GetRandomConnector(tileTo);
        if (connectTo == null) { return; }

        /* 连接逻辑分步说明：
        1. 将目标连接点设为来源连接点的子对象（建立临时父子关系）
        2. 将新房间设为目标连接点的子对象
        3. 重置连接点的本地坐标和旋转
        4. 旋转180度使两个连接点正确对接
        5. 恢复房间的父级关系 */
        
        connectTo.SetParent(connectFrom);
        tileTo.SetParent(connectTo);
        
        // 对齐连接点
        connectTo.localPosition = Vector3.zero;
        connectTo.localRotation = Quaternion.identity;
        connectTo.Rotate(0, 180f, 0); // 反向旋转实现对接
        
        // 恢复层级关系
        tileTo.SetParent(container);
        connectTo.SetParent(tileTo.Find("Connectors"));

        // 记录连接点信息
        generatedTiles.Last().connector = connectFrom.GetComponent<Connector>();
    }

    /// <summary>
    /// 获取指定房间的随机可用连接点
    /// </summary>
    /// <param name="tile">目标房间的Transform</param>
    /// <returns>可用的连接点Transform</returns>
    Transform GetRandomConnector(Transform tile)
    {
        if (tile == null) { return null; }
        
        // 获取所有未连接的连接点
        List<Connector> connectorList = tile.GetComponentsInChildren<Connector>().ToList()
            .FindAll(x => x.isConnected == false);
        
        if (connectorList.Count > 0)
        {
            // 随机选择并标记为已连接
            int connectorIndex = Random.Range(0, connectorList.Count);
            connectorList[connectorIndex].isConnected = true;  
            return connectorList[connectorIndex].transform;
        }
        return null;
    }
}

/* 辅助类说明：
Tile类用于记录房间信息：
- tile: 房间的Transform
- origin: 父房间的Transform
- connector: 使用的连接点组件 */