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
/// 功能包含：
/// - 主路径生成
/// - 分支路径生成
/// - 房间碰撞检测
/// - 动态连接点管理
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    [Header("预制体配置")]
    [Tooltip("起始房间（建议包含多个方向的连接点）")]
    public GameObject startTile;          // 起始房间预制体（通常为入口/电梯）
    
    [Tooltip("普通房间预制体集合（需包含Connector组件）")]
    public GameObject[] tilePrefabs;      // 可随机选择的普通房间预制体
    
    [Tooltip("墙体障碍物预制体（预留功能）")]
    public GameObject[] blockedPrefabs;    // 障碍物预制体集合（当前版本未使用）
    
    [Tooltip("门预制体（预留功能）")]
    public GameObject[] doorPrefabs;      // 门类型预制体集合（当前版本未使用）
    
    [Tooltip("出口房间预制体（预留功能）")]
    public GameObject[] exitPrefabs;       // 终点房间集合（当前版本未使用）

    [Header("连接控制")]
    [Tooltip("前一个连接的房间")]
    Transform tileFrom;                    // 连接源房间（已存在的房间）
    
    [Tooltip("当前要连接的房间")]
    Transform tileTo;                      // 连接目标房间（新生成的房间）
    
    [Tooltip("地牢生成根节点")]
    Transform tileRoot;                    // 整个地牢的父节点
    
    [Tooltip("当前生成容器")]
    Transform container;                   // 当前生成路径的父对象

    [Header("生成参数")]
    [Header("主路径设置")]
    [Range(2,100)]
    [Tooltip("包含起始房间的总长度")]
    public int mainLength = 10;           // 主路径房间数量（含起始房间）
    
    [Header("分支设置")]
    [Range(0,50)]
    [Tooltip("每个分支的最大长度")]
    public int branchLenght = 5;           // 单个分支的最大房间数
    
    [Range(0,25)]
    [Tooltip("要生成的分支数量")]
    public int branchNum = 10;             // 分支路径总数
    
    [Header("高级设置")]
    [Range(0,100)]
    [Tooltip("门生成概率（0-100%）")]
    public int doorPercent = 25;           // 门生成几率（当前版本未实现）
    
    [Range(0,1f)]
    [Tooltip("房间生成间隔时间（秒）")]
    public float constructionDelay;        // 视觉效果延迟
    
    [Header("运行时数据")]
    [Tooltip("已生成房间列表")]
    [SerializeField]
    private List<Tile> generatedTiles = new List<Tile>(); // 所有生成的房间记录
    
    [Tooltip("可用连接点池")]
    [SerializeField]
    private List<Connector> availableConnectors = new List<Connector>(); // 可用于生成分支的连接点

    [Header("调试选项")]
    [Tooltip("启用碰撞体可视化")]
    public bool usingBoxCollider;           // 是否显示碰撞体
    
    [Tooltip("临时启用灯光调试")]
    public bool usingLightForDebugging;     // 调试用灯光开关
    
    [Tooltip("调试后恢复灯光")]
    public bool RestoreLightsAfterDebugging;// 调试后是否恢复原灯光
    private int attempts,maxAttempts = 50;

    /// <summary>
    /// 初始化生成协程
    /// </summary>
    void Start()
    {
        
        // 启动地牢生成流程
        StartCoroutine(DungeonBuild());
    }

    /// <summary>
    /// 地牢生成主协程
    /// 生成流程：
    /// 1. 创建主路径容器
    /// 2. 生成起始房间
    /// 3. 循环生成主路径房间
    /// 4. 收集剩余连接点
    /// 5. 生成分支路径
    /// 6. 执行最终清理
    /// </summary>
    IEnumerator DungeonBuild()
    {
        // 创建主路径父对象
        GameObject goContainer = new GameObject("Main Path");
        container = goContainer.transform;
        container.SetParent(transform);
        
        // 初始化起始房间
        tileRoot = CreatStartTile();
        tileTo = tileRoot;

        // 主路径生成循环
        for (int i = 0; i < mainLength - 1; i++)
        {
            
            // 生成间隔（可视化效果）
            yield return new WaitForSeconds(constructionDelay);
            // 更新房间指针
            tileFrom = tileTo;
            tileTo = CreatTile();
            // 连接当前房间
            ConnectTiles();
            // 执行碰撞检测（当前为空实现）
            CollisionCheck();
            if(attempts >= maxAttempts){break;}
        }

        // 收集未使用的连接点用于分支生成
        foreach(Connector connector in container.GetComponentsInChildren<Connector>())
        {
            if (!connector.isConnected && !availableConnectors.Contains(connector))
            {
                availableConnectors.Add(connector);
            }
        }

        // 分支生成阶段
        for (int b = 0; b < branchNum; b++)
        {
            if (availableConnectors.Count > 0) 
            {
                // 创建分支容器
                goContainer = new GameObject("Branch" + (b+1));
                container = goContainer.transform;
                container.SetParent(transform);

                // 随机选择分支起点
                int connectorIndex = Random.Range(0, availableConnectors.Count);
                tileRoot = availableConnectors[connectorIndex].transform.parent.parent;
                availableConnectors.RemoveAt(connectorIndex);

                // 分支路径生成
                tileTo = tileRoot;
                for (int i = 0; i < branchLenght - 1; i++)
                {
                    yield return new WaitForSeconds(constructionDelay);
                    tileFrom = tileTo;
                    tileTo = CreatTile();
                    ConnectTiles();
                    CollisionCheck();
                    if(attempts >= maxAttempts){ break;}
                }
            }
            else {break;}
        }

        // 最终清理
        CleanupBoxes();
    }

    /// <summary>
    /// 重置场景（重新生成地牢）
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
        // 实例化起始房间
        GameObject tile = Instantiate(startTile, Vector3.zero, Quaternion.identity, container);
        tile.name = "Start Room";
        
        // 初始化房间数据
        generatedTiles.Add(new Tile(tile.transform, null));

        return tile.transform;
    }

    /// <summary>
    /// 创建普通房间
    /// 流程：
    /// 1. 随机选择预制体
    /// 2. 实例化房间
    /// 3. 随机旋转方向
    /// 4. 记录生成关系
    /// </summary>
    Transform CreatTile()
    {
        // 随机选择房间类型
        int tileIndex = Random.Range(0, tilePrefabs.Length);
        GameObject tile = Instantiate(tilePrefabs[tileIndex], Vector3.zero, Quaternion.identity, container);
        tile.name = tilePrefabs[tileIndex].name;
        
        // 设置随机朝向
        float yRot = Random.Range(0, 4) * 90f;
        tile.transform.Rotate(0, yRot, 0);

        // 查找父房间并记录
        Transform origin = generatedTiles.Find(x => x.tile == tileFrom).tile;
        generatedTiles.Add(new Tile(tile.transform, origin));

        return tile.transform;
    }

    /// <summary>
    /// 连接两个房间的核心方法
    /// 连接过程：
    /// 1. 获取双方的可用连接点
    /// 2. 建立临时父子关系
    /// 3. 对齐连接点位置
    /// 4. 旋转对接方向
    /// 5. 恢复层级关系
    /// </summary>
    void ConnectTiles()
    {
        // 获取连接点
        Transform connectFrom = GetRandomConnector(tileFrom);
        if (!connectFrom) return;
        Transform connectTo = GetRandomConnector(tileTo);
        if (!connectTo) return;

        // 建立临时层级
        connectTo.SetParent(connectFrom);
        tileTo.SetParent(connectTo);

        // 对齐连接点
        connectTo.localPosition = Vector3.zero;
        connectTo.localRotation = Quaternion.identity;
        connectTo.Rotate(0, 180f, 0);  // 180度旋转实现无缝对接

        // 恢复层级结构
        tileTo.SetParent(container);
        connectTo.SetParent(tileTo.Find("Connectors"));

        // 记录连接信息
        generatedTiles.Last().connector = connectFrom.GetComponent<Connector>();
    }

    /// <summary>
    /// 获取随机可用连接点
    /// </summary>
    Transform GetRandomConnector(Transform tile)
    {
        if (!tile) return null;

        // 筛选可用连接点
        var connectors = tile.GetComponentsInChildren<Connector>()
            .Where(c => !c.isConnected).ToList();

        if (connectors.Count == 0) return null;

        // 随机选择并标记
        Connector selected = connectors[Random.Range(0, connectors.Count)];
        selected.isConnected = true;

        // 调试模式下添加碰撞体
        if (tile == tileFrom && !tile.GetComponent<BoxCollider>())
        {
            var box = tile.gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
        }

        return selected.transform;
    }

    /// <summary>
    /// 清理调试用碰撞体
    /// </summary>
    void CleanupBoxes()
    {
        if (usingBoxCollider) return;

        foreach (Tile tile in generatedTiles)
        {
            var box = tile.tile.GetComponent<BoxCollider>();
            if (box) Destroy(box);
        }
    }
    void CollisionCheck()
    {
        BoxCollider box = tileTo.GetComponent<BoxCollider>();
        if (box == null)
        {
            // 创建临时触发器用于碰撞检测（不会触发物理反馈）
            box = tileTo.gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
        }
        
        // 计算碰撞体中心点的世界坐标偏移（考虑房间旋转的影响）
        Vector3 offset = (tileTo.right * box.center.x) 
                    + (tileTo.up * box.center.y) 
                    + (tileTo.forward * box.center.z);
        
        // 获取碰撞体的半尺寸（OverlapBox需要半长宽高）
        Vector3 halfExtents = box.bounds.extents;
        List<Collider> hits = Physics.OverlapBox(
            tileTo.position + offset, 
            halfExtents, 
            Quaternion.identity, 
            LayerMask.GetMask("Tile")
        ).ToList();

        // 碰撞结果分析
        if (hits.Count > 0)
        {
            // 排除当前连接的父房间（tileFrom）和自身（tileTo）
            // 当检测到其他房间的碰撞体时，增加尝试次数
            if(hits.Exists(x => x.transform != tileFrom && x.transform != tileTo))
            {
                attempts++;  // 类级别变量，记录当前生成尝试次数
               
                int toIndex = generatedTiles.FindIndex(x => x.tile == tileTo);
                if(generatedTiles[toIndex].connector !=null)
                {
                    generatedTiles[toIndex].connector.isConnected = false;
                }
                generatedTiles.RemoveAt(toIndex);
                DestroyImmediate(tileTo.gameObject);

                if(attempts >= maxAttempts)
                {
                    int fromIndex = generatedTiles.FindIndex(x => x.tile == tileFrom);
                    Tile myTileFrom = generatedTiles[fromIndex];
                    if(tileFrom != tileRoot)
                    {
                        if(myTileFrom.connector !=null)
                        {
                            myTileFrom.connector.isConnected = false;
                        }
                        availableConnectors.RemoveAll(x =>x.transform.parent.parent == tileFrom);
                        generatedTiles.RemoveAt(fromIndex);
                        DestroyImmediate(tileFrom.gameObject);

                        if(myTileFrom.origin != tileRoot)
                        {
                            tileFrom = myTileFrom.origin;//将tileFrom回退
                        }
                        else if(container.name.Contains("Main"))
                        {
                            if(myTileFrom.origin != null )
                            {
                                tileRoot = myTileFrom.origin;
                                tileFrom = tileRoot;
                            }
                        }
                        else if(availableConnectors.Count > 0)
                        {
                            int availableIndex = Random.Range(0, availableConnectors.Count);
                            tileRoot = availableConnectors[availableIndex].transform.parent.parent;
                            availableConnectors.RemoveAt(availableIndex);
                            tileFrom = tileRoot;
                        }
                        else
                        {return;}
                    }
                    else if(container.name.Contains("Main"))
                    {
                        if (myTileFrom.origin != null)
                        {
                            tileRoot = myTileFrom.origin;
                            tileFrom = tileRoot;
                        }
                    }
                    else if(availableConnectors.Count > 0)
                    {
                        int availableIndex = Random.Range(0, availableConnectors.Count);
                            tileRoot = availableConnectors[availableIndex].transform.parent.parent;
                            availableConnectors.RemoveAt(availableIndex);
                            tileFrom = tileRoot;
                    }
                    else
                    {return;}
                }

                if(tileFrom !=null)
                {
                    tileTo = CreatTile();
                    ConnectTiles();
                    CollisionCheck();
                }
            }else{attempts = 0;}
        }
    }
}

/* 辅助类说明：
Tile类用于记录房间信息：
- tile: 房间的Transform
- origin: 父房间的Transform
- connector: 使用的连接点组件 */