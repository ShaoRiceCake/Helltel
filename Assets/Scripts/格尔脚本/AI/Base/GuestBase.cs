using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using NPBehave;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Helltal.Gelercat
{
    public class GuestBase: MonoBehaviour,IDie
    {
        [Header("最大生成数")] public int maxSpawnCount = 100;
        [Header("最大生命值")] public int maxHealth = 15;
        [Header("当前生命值")] public NetworkVariable<float> curHealth = new NetworkVariable<float>();
        [Header("熵值")] public float entropyValue = 0f;
        
        [Header("检测工具")]
        public PlayerItemDetector playerItemDetector ; 
        
        public bool debugStopBehaviorTree = false; //调试用，是否停止行为树

        public bool debugging = false; // 是否开启单机调试模式

        public NavPointsManager navPointsManager;  // 导航点管理器

        public NavMeshAgent agent;  // 导航代理

        public GuestPresenter presenter;  // 表现层的api，动画用animator控制器控制

        public NPBehave.Root BehaviorTree; // 显式声明命名空间

        public GameObject _curTarget;

        protected virtual void Update()
        {
        }
        
        protected virtual void LateUpdate()
        {
            if (debugStopBehaviorTree)
            {
                if (BehaviorTree.IsActive)
                {
                    BehaviorTree.Stop();
                }
            }
            else
            {
                if (BehaviorTree.CurrentState == NPBehave.Node.State.INACTIVE && !BehaviorTree.IsActive)
                {
                    BehaviorTree.Start();
                }
            }
        }
        
        protected bool IsEnemyCanSee()
        {
            return playerItemDetector && playerItemDetector.isDetectPlayer;
        }

        protected virtual void Awake()
        {
            // if (!IsHost && NetworkManager.Singleton) return;
            if (ShouldUseNavMeshAgent())
            {
                agent = !GetComponent<NavMeshAgent>() ? gameObject.AddComponent<NavMeshAgent>() : GetComponent<NavMeshAgent>();
                Debug.Log(agent);
            }

            navPointsManager = GameObject.Find("NavPointManager").GetComponent<NavPointsManager>();
            if (!navPointsManager)
            {
                Debug.LogError("请先在场景中添加导航点管理器");
            }

            presenter = !GetComponent<GuestPresenter>() ? this.gameObject.AddComponent<GuestPresenter>() : GetComponent<GuestPresenter>();
        }
        
        protected virtual void Start()
        {
            _curTarget = GameObject.Find("MoveBodyBall");
            if (!_curTarget)
                Debug.LogError("Player Not Found!");
            
            curHealth.Value = maxHealth;
        }
        
        protected void NegativeTo(Vector3 target)
        {
            agent.SetDestination(target);
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
                return agent && agent.isOnNavMesh;
            }
            catch
            {
                return false;
            }
        }
        protected virtual void OnDestroy()
        {
            if (BehaviorTree is not { IsActive: true }) return;
            BehaviorTree.Stop();
        }


        protected virtual void Die()
        {
            
        }
        public bool IsDead { get; set; }
    }
}