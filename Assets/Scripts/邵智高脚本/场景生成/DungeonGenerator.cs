using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using System.ComponentModel;
using Unity.Collections;
using System.Linq.Expressions;
using Unity.Netcode;
using UnityEngine.Events;

/// <summary>
/// 地牢生成器核心类
/// 实现基于连接点的程序化地牢生成系统
/// 功能包含：
/// - 主路径生成
/// - 分支路径生成
/// - 房间碰撞检测
/// - 动态连接点管理
/// </summary>
public enum DungeonState { inActive, generatingMain, generatingBranches, cleanup, completed }

public class DungeonGenerator : NetworkBehaviour
{
    const float ROOM_SCALE = 1.3f;
    [Header("电梯配置")]
    [Tooltip("场景中固定的电梯Tile（需包含Connector组件）")]
    public Transform elevatorTile; // 电梯
    [Tooltip("起始房间（建议包含多个方向的连接点）")]
    public GameObject startTile;          // 起始走廊预制体

    [Tooltip("普通房间预制体集合（需包含Connector组件）")]
    public GameObject[] tilePrefabs;      // 可随机选择的普通房间预制体

    [Tooltip("墙体障碍物预制体（预留功能）")]
    public GameObject[] blockedPrefabs;    // 障碍物预制体集合

    [Tooltip("门预制体（预留功能）")]
    public GameObject[] doorPrefabs;      // 门类型预制体集合

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
    [Range(2, 100)]
    [Tooltip("包含起始房间的总长度")]
    private int mainLength = 10;           // 主路径房间数量（含起始房间）

    [Header("分支设置")]
    [Range(0, 50)]
    [Tooltip("每个分支的最大长度")]
    private int branchLength = 5;           // 单个分支的最大房间数

    [Range(0, 25)]
    [Tooltip("要生成的分支数量")]
    private int branchNumber = 6;             // 分支路径总数

    [Header("高级设置")]
    [Range(0, 100)]
    [Tooltip("门生成概率（0-100%）")]
    public int doorPercent = 25;           // 门生成几率（当前版本未实现）

    // [Range(0, 1f)]
    // [Tooltip("房间生成间隔时间（秒）")]
    // public float constructionDelay;        // 视觉效果延迟

    [Header("运行时数据")]
    [Tooltip("已生成房间列表")]
    [SerializeField]
    private List<Tile> generatedTiles = new List<Tile>(); // 所有生成的房间记录

    [Tooltip("可用连接点池")]
    [SerializeField]
    private List<Connector> availableConnectors = new List<Connector>(); // 可用于生成分支的连接点
    [SerializeField]
    private DungeonState dungeonState = DungeonState.inActive;

    private int attempts, maxAttempts = 100;
    // 生成完成事件
    public static System.Action<DungeonGenerator> OnDungeonBuildCompleted;


    //Queue<int> randomque = new Queue<int>();
    public static DungeonGenerator Instance { get; private set; }

    private void Awake() 
    {
        if (Instance == null) 
        {
            Instance = this;
        }
    }
    /// <summary>
    /// 初始化生成协程
    /// </summary>
    void Start()
    {
        // if (NetworkManager.Singleton)
        // {
        //     randomque.Clear();
        //     if (IsHost)
        //     {
        //         DungeonBuild();
        //     }
        //     // 启动地牢生成流程
            
        // }
        // else
        // {
        //     // 启动地牢生成流程
        //     DungeonBuild();
        // }
    }
    public void ReSetDungeonValue()
    {
        mainLength = GameController.Instance._gameData.Level*1+4;
        branchLength = GameController.Instance._gameData.Level*1+2;
        branchNumber = GameController.Instance._gameData.Level*1+2;
        DungeonBuild();
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
    public void DungeonBuild()
    { 
        // 创建主路径父对象
        GameObject goContainer = new GameObject("Main Path");
        container = goContainer.transform;
        container.SetParent(transform);
        // 初始化起始房间
        tileRoot = CreatStartTile();
        tileTo = tileRoot;
        if (elevatorTile != null)
        {
            // 调用专用连接方法
            ConnectElevator(tileRoot, elevatorTile);

            // 强制刷新连接状态（重要！）
            tileRoot.GetComponentInChildren<Connector>().isConnected = true;
            elevatorTile.GetComponentInChildren<Connector>().isConnected = true;
        }
        dungeonState = DungeonState.generatingMain;
        // 主路径生成循环
        for (int i = 0; i < mainLength - 1; i++)
        {

            // 生成间隔（可视化效果）
            //yield return new WaitForSeconds(constructionDelay);
            // 更新房间指针
            tileFrom = tileTo;
            tileTo = CreatTile();
            // 连接当前房间
            ConnectTiles();
            // 执行碰撞检测（当前为空实现）
            CollisionCheck();
            if (attempts >= maxAttempts) { break; }
        }

        // 收集未使用的连接点用于分支生成
        foreach (Connector connector in container.GetComponentsInChildren<Connector>())
        {
            if (!connector.isConnected && !availableConnectors.Contains(connector))
            {
                availableConnectors.Add(connector);
            }
        }
        dungeonState = DungeonState.generatingBranches;
        // 分支生成阶段
        for (int b = 0; b < branchNumber; b++)
        {
            if (availableConnectors.Count > 0)
            {
                // 创建分支容器
                goContainer = new GameObject("Branch" + (b + 1));
                container = goContainer.transform;
                container.SetParent(transform);
                int connectorIndex;
                // 随机选择分支起点
                // if (NetworkManager.Singleton)
                // {
                //     if (IsHost)
                //     {
                //         connectorIndex = Random.Range(0, availableConnectors.Count);
                //         GiveRandomClientRpc(connectorIndex);
                //     }
                //     else
                //     {
                //         connectorIndex = randomque.Dequeue();
                //     }
                // }
                // else
                // {
                //     connectorIndex = Random.Range(0, availableConnectors.Count);
                // }
                connectorIndex = Random.Range(0, availableConnectors.Count);
                tileRoot = availableConnectors[connectorIndex].transform.parent.parent;
                availableConnectors.RemoveAt(connectorIndex);
                // 分支路径生成
                tileTo = tileRoot;
                for (int i = 0; i < branchLength ; i++)
                {
                    //yield return new WaitForSeconds(constructionDelay);
                    tileFrom = tileTo;
                    tileTo = CreatTile();
                    ConnectTiles();
                    CollisionCheck();
                    if (attempts >= maxAttempts) { break; }
                }
            }
            else { break; }
        }

        // 最终清理
        dungeonState = DungeonState.cleanup;
        //CleanupBoxes();
        BlockedPassages();
        dungeonState = DungeonState.completed;
        // if (IsHost)
        // {
        //     StartSpawnClientRpc();
        // }

        // 触发生成完毕事件，广播通知其他组件进行同步
        OnDungeonBuildCompleted?.Invoke(this);
    }
    /// <summary>
    /// 重置场景（重新生成地牢）
    /// </summary>
    public void UpdateDungeon()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 创建起始房间
    /// </summary>
    Transform CreatStartTile()
    {
        // 实例化起始房间
        GameObject tile = Instantiate(startTile, Vector3.zero, Quaternion.identity, container);
        tile.transform.localScale = Vector3.one * ROOM_SCALE;
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
        int tileIndex;
        tileIndex = Random.Range(0, tilePrefabs.Length);
        // if (NetworkManager.Singleton)
        // {
        //     if (IsHost)
        //     {
        //         tileIndex = Random.Range(0, tilePrefabs.Length);
        //         GiveRandomClientRpc(tileIndex);
        //     }
        //     else
        //     {
        //         tileIndex = randomque.Dequeue();
        //     }
        // }
        // else
        // {
        //     tileIndex = Random.Range(0, tilePrefabs.Length);
        // }
        GameObject tile = Instantiate(tilePrefabs[tileIndex], Vector3.zero, Quaternion.identity, container);
        tile.transform.localScale = Vector3.one * ROOM_SCALE;
        tile.name = tilePrefabs[tileIndex].name;
        int yRotRandom;
        // 设置随机朝向
        // float yRot;
        // yRotRandom = Random.Range(0, 4);
        // yRot = yRotRandom * 90f;
        // // if (NetworkManager.Singleton)
        // // {
        // //     if (IsHost)
        // //     {
        // //         yRotRandom = Random.Range(0, 4);
        // //         yRot = yRotRandom * 90f;
        // //         GiveRandomClientRpc(yRotRandom);
        // //     }
        // //     else
        // //     {
        // //         yRot = randomque.Dequeue() * 90f;
        // //     }
        // // }
        // // else
        // // {
        // //     yRotRandom = Random.Range(0, 4);
        // //     yRot = yRotRandom * 90f;
        // // }

        // tile.transform.Rotate(0, yRot, 0);

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
        SpawnDoor(connectFrom.GetComponent<Connector>(),
             connectTo.GetComponent<Connector>());
    }
    void ConnectElevator(Transform startRoom, Transform elevator)
    {
        // 获取双方的连接点
        Connector elevatorConnector = elevator.GetComponentInChildren<Connector>();
        Connector startConnector = startRoom.GetComponentInChildren<Connector>();

        if (elevatorConnector == null || startConnector == null) return;

        // 计算缩放补偿因子
        Vector3 scaleCompensation = new Vector3(
            1 / elevatorConnector.transform.lossyScale.x,
            1 / elevatorConnector.transform.lossyScale.y,
            1 / elevatorConnector.transform.lossyScale.z
        );

        // 临时父子关系（带缩放补偿）
        startRoom.SetParent(elevatorConnector.transform);

        // 精确对齐逻辑（考虑缩放）
        startRoom.localRotation = Quaternion.Euler(0, 180f, 0) *
                                Quaternion.Inverse(startConnector.transform.localRotation);

        // 计算基于实际缩放的位置偏移
        Vector3 scaledOffset = (elevatorConnector.transform.forward * ROOM_SCALE) *
                         (startConnector.transform.localPosition.magnitude +
                          elevatorConnector.transform.localPosition.magnitude);

        // 应用缩放补偿
        //offset = Vector3.Scale(offset, scaleCompensation);

        // 恢复层级关系
        startRoom.SetParent(container);

        // 最终位置计算
        startRoom.position = elevator.position + scaledOffset;
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

        int connectorRandom;
        connectorRandom = Random.Range(0, connectors.Count);
        // if (NetworkManager.Singleton)
        // {
        //     if (IsHost)
        //     {
        //         connectorRandom = Random.Range(0, connectors.Count);
        //         GiveRandomClientRpc(connectorRandom);
        //     }
        //     else
        //     {
        //         connectorRandom = randomque.Dequeue();
        //     }
        // }
        // else
        // {
        //     connectorRandom = Random.Range(0, connectors.Count);
        // }

        Connector selected = connectors[connectorRandom];

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
    // void CleanupBoxes()
    // {
    //     if (usingBoxCollider) return;

    //     foreach (Tile tile in generatedTiles)
    //     {
    //         var box = tile.tile.GetComponent<BoxCollider>();
    //         if (box) Destroy(box);
    //     }
    // }
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
            LayerMask.GetMask("Floor")
        ).ToList();

        // 碰撞结果分析
        if (hits.Count > 0)
        {
            // 排除当前连接的父房间（tileFrom）和自身（tileTo）
            // 当检测到其他房间的碰撞体时，增加尝试次数
            if (hits.Exists(x => x.transform != tileFrom && x.transform != tileTo))
            {
                attempts++;  // 类级别变量，记录当前生成尝试次数

                int toIndex = generatedTiles.FindIndex(x => x.tile == tileTo);
                if (generatedTiles[toIndex].connector != null)
                {
                    generatedTiles[toIndex].connector.isConnected = false;
                }
                generatedTiles.RemoveAt(toIndex);
                DestroyImmediate(tileTo.gameObject);

                if (attempts >= maxAttempts)
                {
                    int fromIndex = generatedTiles.FindIndex(x => x.tile == tileFrom);
                    Tile myTileFrom = generatedTiles[fromIndex];
                    if (tileFrom != tileRoot)
                    {
                        if (myTileFrom.connector != null)
                        {
                            myTileFrom.connector.isConnected = false;
                        }
                        availableConnectors.RemoveAll(x => x.transform.parent.parent == tileFrom);
                        generatedTiles.RemoveAt(fromIndex);
                        DestroyImmediate(tileFrom.gameObject);

                        if (myTileFrom.origin != tileRoot)
                        {
                            tileFrom = myTileFrom.origin;//将tileFrom回退
                        }
                        else if (container.name.Contains("Main"))
                        {
                            if (myTileFrom.origin != null)
                            {
                                tileRoot = myTileFrom.origin;
                                tileFrom = tileRoot;
                            }
                        }
                        else if (availableConnectors.Count > 0)
                        {
                            int availableIndex;
                            availableIndex = Random.Range(0, availableConnectors.Count);
                            // if (NetworkManager.Singleton)
                            // {
                            //     if (IsHost)
                            //     {
                            //         availableIndex = Random.Range(0, availableConnectors.Count);
                            //         GiveRandomClientRpc(availableIndex);
                            //     }
                            //     else
                            //     {
                            //         availableIndex = randomque.Dequeue();
                            //     }
                            // }
                            // else
                            // {
                            //     availableIndex = Random.Range(0, availableConnectors.Count);
                            // }

                            tileRoot = availableConnectors[availableIndex].transform.parent.parent;
                            availableConnectors.RemoveAt(availableIndex);
                            tileFrom = tileRoot;
                        }
                        else
                        { return; }
                    }
                    else if (container.name.Contains("Main"))
                    {
                        if (myTileFrom.origin != null)
                        {
                            tileRoot = myTileFrom.origin;
                            tileFrom = tileRoot;
                        }
                    }
                    else if (availableConnectors.Count > 0)
                    {
                        int availableIndex;
                        availableIndex = Random.Range(0, availableConnectors.Count);
                        // if (NetworkManager.Singleton)
                        // {
                        //     if (IsHost)
                        //     {
                        //         availableIndex = Random.Range(0, availableConnectors.Count);
                        //         GiveRandomClientRpc(availableIndex);
                        //     }
                        //     else
                        //     {
                        //         availableIndex = randomque.Dequeue();
                        //     }
                        // }
                        // else
                        // {
                        //     availableIndex = Random.Range(0, availableConnectors.Count);
                        // }

                        tileRoot = availableConnectors[availableIndex].transform.parent.parent;
                        availableConnectors.RemoveAt(availableIndex);
                        tileFrom = tileRoot;
                    }
                    else
                    { return; }
                }

                if (tileFrom != null)
                {
                    tileTo = CreatTile();
                    ConnectTiles();
                    CollisionCheck();
                }
            }
            else { attempts = 0; }
        }
    }
    void BlockedPassages()
    {
        foreach (Connector connector in transform.GetComponentsInChildren<Connector>())
        {
            if (connector.isConnected == false)
            {
                Vector3 pos = connector.transform.position;

                int wallIndex;
                wallIndex = Random.Range(0, blockedPrefabs.Length);
                // if (NetworkManager.Singleton)
                // {
                //     if (IsHost)
                //     {
                //         wallIndex = Random.Range(0, blockedPrefabs.Length);
                //         GiveRandomClientRpc(wallIndex);
                //     }
                //     else
                //     {
                //         wallIndex = randomque.Dequeue();
                //     }
                // }
                // else
                // {
                //     wallIndex = Random.Range(0, blockedPrefabs.Length);
                // }


                GameObject gowall = Instantiate(blockedPrefabs[wallIndex], pos, connector.transform.rotation, connector.transform);
                gowall.name = blockedPrefabs[wallIndex].name;
            }
        }
    }
    /// <summary>
    /// 在连接点生成门（根据概率）
    /// </summary>
    void SpawnDoor(Connector from, Connector to)
    {
        if (doorPercent <= 0) return;

        int doorPercentRandom;
        doorPercentRandom = Random.Range(0, 100);
        // if (NetworkManager.Singleton)
        // {
        //     if (IsHost)
        //     {
        //         doorPercentRandom = Random.Range(0, 100);
        //         GiveRandomClientRpc(doorPercentRandom);
        //     }
        //     else
        //     {
        //         doorPercentRandom = randomque.Dequeue();
        //     }
        // }
        // else
        // {
        //     doorPercentRandom = Random.Range(0, 100);
        // }


        // 概率检查
        if (doorPercentRandom < doorPercent)
        {
            // 避免重复生成
            if (from.hasDoor || to.hasDoor) return;

            // 随机选择门预制体
            int doorIndex;
            doorIndex = Random.Range(0, doorPrefabs.Length);
            // if (NetworkManager.Singleton)
            // {
            //     if (IsHost)
            //     {
            //         doorIndex = Random.Range(0, doorPrefabs.Length);
            //         GiveRandomClientRpc(doorIndex);
            //     }
            //     else
            //     {
            //         doorIndex = randomque.Dequeue();
            //     }
            // }
            // else
            // {
            //     doorIndex = Random.Range(0, doorPrefabs.Length);
            // }

            GameObject doorPrefab = doorPrefabs[doorIndex];

            // 在from连接点生成门
            GameObject door = Instantiate(doorPrefab, from.transform.position,
                    from.transform.rotation, from.transform);

            //door.transform.localScale = Vector3.one * ROOM_SCALE;
            // 标记已生成门
            from.hasDoor = to.hasDoor = true;
        }
    }
    public void SetDungeonValue()
    {

    }

    // [Rpc(SendTo.NotMe)]
    // void GiveRandomClientRpc(int random)
    // {
    //     randomque.Enqueue(random);
    // }
    // [Rpc(SendTo.NotMe)]
    // void StartSpawnClientRpc()
    // {
    //     DungeonBuild();
    // }
}

/* 辅助类说明：
Tile类用于记录房间信息：
- tile: 房间的Transform
- origin: 父房间的Transform
- connector: 使用的连接点组件 */