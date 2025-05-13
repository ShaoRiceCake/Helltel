using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using NPBehave;
using System.Collections.Generic;
namespace Helltal.Gelercat
{
    public class GuestBase : NetworkBehaviour
    {
        NetworkVariable<Vector3> _syncPos = new NetworkVariable<Vector3>();
        NetworkVariable<Quaternion> _syncRot = new NetworkVariable<Quaternion>();

        // 同步transform，基础数据
        // 基础属性
        [Header("最大生成数")] public int maxSpawnCount = 100;
        [Header("最大生命值")] public int maxHealth = 15;
        [Header("当前生命值")] public NetworkVariable<float> curHealth = new NetworkVariable<float>();
        [Header("熵值")] public float entropyValue = 0f;



        public bool DEBUG_STOP_BEHAVIOR_TREE = false; //调试用，是否停止行为树

        public bool Debugging = false; // 是否开启单机调试模式

        public NavPointsManager navPointsManager;  // 导航点管理器

        public NavMeshAgent agent;  // 导航代理

        public GuestPresenter presenter;  // 表现层的api，动画用animator控制器控制

        /// TODO：AudioPresident
        public GuestSensor sensor;  // 
        protected Root behaviorTree;

        public List<GameObject> EnemyList = new List<GameObject>(); //敌人列表

        public NetworkVariable<AIState> aiState = new NetworkVariable<AIState>(AIState.LIVE);
        // /// <summary>
        // /// 联机扣血
        // /// </summary>
        // /// <param name="damage"></param>
        // [Rpc(SendTo.Server)]
        // public void TakeDamageServerRpc(float damage)
        // {
        //     curHealth.Value -= damage;
        // }
        /// <summary>
        /// 联机切换状态
        /// </summary>
        /// <param name="newstate"></param>
        [Rpc(SendTo.Server)]
        public void StateChangeServerRpc(AIState newstate)
        {
            aiState.Value = newstate;
        }

        protected virtual void Update()
        {
            // if (!IsHost)
            // {
            //     transform.position = _syncPos.Value;
            //     transform.rotation = _syncRot.Value;
            //     return;
            // }
            // else
            // {
            //     UpdateTransformRpc(transform.position, transform.rotation);
            // }
        }

        // protected virtual void LateUpdate()
        // {
        //     if(DEBUG_STOP_BEHAVIOR_TREE && behaviorTree != null)
        //     {
        //         behaviorTree.Stop();
        //     }
        //     else if (behaviorTree != null && !behaviorTree.IsActive)
        //     {
        //         behaviorTree.Start();
        //     }
        // }


        protected virtual void LateUpdate()
        {
            // if (behaviorTree == null) return;

            if (DEBUG_STOP_BEHAVIOR_TREE)
            {
                if (behaviorTree.IsActive)
                {
                    behaviorTree.Stop();
                }
            }
            else
            {
                // 只有完全停止后才能重新启动
                if (behaviorTree.CurrentState == NPBehave.Node.State.INACTIVE && !behaviorTree.IsActive)
                {
                    behaviorTree.Start();
                }
            }
        }



        [Rpc(SendTo.Server)]
        void UpdateTransformRpc(Vector3 pos, Quaternion rot)
        {
            _syncPos.Value = pos;
            _syncRot.Value = rot;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Init();
        }

        protected virtual void Init()
        {
            // if (!IsHost && NetworkManager.Singleton) return;

            // TakeDamageServerRpc(maxSpawnCount);
        }
        /// <summary>
        /// 在这里写组件初始化逻辑 
        /// </summary>
        protected virtual void Awake()
        {
            // if (!IsHost && NetworkManager.Singleton) return;
            if (ShouldUseNavMeshAgent())
            {
                agent = GetComponent<NavMeshAgent>() == null ? gameObject.AddComponent<NavMeshAgent>() : GetComponent<NavMeshAgent>();
                Debug.Log(agent);
            }

            navPointsManager = GameObject.Find("NavPointManager").GetComponent<NavPointsManager>();
            if (navPointsManager == null)
            {
                Debug.LogError("请先在场景中添加导航点管理器");
            }

            if (GetComponent<GuestPresenter>() == null)
            {
                presenter = this.gameObject.AddComponent<GuestPresenter>();
            }
            else
            {
                presenter = GetComponent<GuestPresenter>();
            }


        }

        protected virtual void Start()
        {
            curHealth.Value = maxHealth;

        }


        protected void NegativeTo(Vector3 target)
        {
            // if (!IsHost && NetworkManager.Singleton) return;
            agent.SetDestination(target);
        }
        void OnDrawGizmos()
        {
            if (agent == null) return;
            Vector3 targetPosition = agent.destination;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawSphere(targetPosition, 0.5f);

        }
        // interface IGetBehaviorTree
        protected virtual Root GetBehaviorTree()
        {
            return new Root(
                new WaitUntilStopped()
            );
        }

        protected virtual bool ShouldUseNavMeshAgent()
        {
            return true;
        }

        protected bool IsNavAgentOnNavmesh()
        {
            try
            {
                return agent != null && agent.isOnNavMesh;
            }
            catch
            {
                return false;
            }
        }
    }
}