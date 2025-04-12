using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System;
using NPBehave;
using UnityEngine.InputSystem;
using UnityEditor.SceneManagement;

namespace Helltal.Gelercat
{
    public class GuestBase : NetworkBehaviour
    {
        // 同步transform，基础数据
        // 基础属性
        [Header("最大生命值")] public float maxHealth = 15f;
        [Header("当前生命值")] public NetworkVariable<float> curHealth = new NetworkVariable<float>();
        [Header("熵值")] public float entropyValue = 0f;

        public bool Debugging = false; // 是否开启调试模式
        // 基础控件        
        public GameObject[] players;
        
        public NavPointsManager navPointsManager;  // 导航点管理器

        public NavMeshAgent agent;  // 导航代理

        public GuestPresenter presenter;  // 表现层的api，动画用animator控制器控制

        /// TODO：AudioPresident
        public GuestSensor sensor;  // 


        public NetworkVariable<AIState> aiState = new NetworkVariable<AIState>(AIState.LIVE);
        /// <summary>
        /// 联机扣血
        /// </summary>
        /// <param name="damage"></param>
        [Rpc(SendTo.Server)]
        public void TakeDamageServerRpc(float damage)
        {
            curHealth.Value = damage;
        }
        /// <summary>
        /// 联机切换状态
        /// </summary>
        /// <param name="newstate"></param>
        [Rpc(SendTo.Server)]
        public void StateChangeServerRpc(AIState newstate)
        {
            aiState.Value = newstate;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Init();
        }

        protected virtual void Init()
        {
            if (!IsHost && NetworkManager.Singleton) return;
        }

        protected virtual void Start()
        {
            if (!IsHost && NetworkManager.Singleton) return;

            agent = GetComponent<NavMeshAgent>()==null? gameObject.AddComponent<NavMeshAgent>() : GetComponent<NavMeshAgent>();
            navPointsManager = GameObject.Find("NavPointManager").GetComponent<NavPointsManager>();
            if (navPointsManager == null)
            {
                Debug.LogError("请先在场景中添加导航点管理器");
            }
            players = GameObject.FindGameObjectsWithTag("Player");
            if(GetComponent<GuestPresenter>() == null)
            {
                presenter = this.gameObject.AddComponent<GuestPresenter>();
            }
            else
            {
                presenter = GetComponent<GuestPresenter>();
            }
            sensor = GetComponent<GuestSensor>();
            if (sensor == null)
            {
                sensor = this.gameObject.AddComponent<GuestSensor>();

            }
        }

        protected void NegativeTo(Vector3 target)
        {
            if (!IsHost && NetworkManager.Singleton) return;
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
        public virtual Root GetBehaviorTree()
        {
            return null;
        }
    }
}