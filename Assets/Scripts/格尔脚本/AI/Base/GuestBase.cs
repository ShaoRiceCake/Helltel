using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System;
using NPBehave;
using UnityEngine.InputSystem;

namespace Helltal.Gelercat
{
    public class GuestBase : NetworkBehaviour
    {
        // 同步transform，基础数据
        // 基础属性
        [Header("最大生命值")] public float maxHealth = 15f;
        [Header("当前生命值")] public NetworkVariable<float> curHealth = new NetworkVariable<float>();
        [Header("熵值")] public float entropyValue = 0f;



        // 基础控件        
        public GameObject[] players;
        public Pathlist path;  // 场景中的导航点列表

        public NavMeshAgent agent;  // 导航代理

        public GuestPresenter presenter;  // 表现层的api，动画用animator控制器控制

        /// TODO：AudioPresident
        public GuestSensor sensor;  // 


        public NetworkVariable<AIState> aiState = new NetworkVariable<AIState>(AIState.LIVE);



        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Init();
        }

        protected virtual void Init()
        {

#if UNITY_EDITOR
if (Application.isEditor)
{
    NetworkManager.Singleton.StartHost();
}
#endif


            if (!IsHost) {
                Debug.Log("is not Host"); 
                return;
            }
            Debug.Log("is Host");

        }

        protected virtual void Start()
        {
            agent = GetComponent<NavMeshAgent>()==null? gameObject.AddComponent<NavMeshAgent>() : GetComponent<NavMeshAgent>();
            path = GameObject.Find("PathList").GetComponent<Pathlist>();
            if (path == null)
            {
                Debug.LogError("请先在场景中添加导航点管理器");
            }
            players = GameObject.FindGameObjectsWithTag("Player");
            presenter = GetComponent<GuestPresenter>();

            sensor = GetComponent<GuestSensor>();
            if (sensor == null)
            {
                sensor = this.gameObject.AddComponent<GuestSensor>();

            }
        }



        protected void NegativeTo(Vector3 target)
        {
            // if (IsHost)
            {
                agent.SetDestination(target);
            }
        }
        void OnDrawGizmos()
        {
            if (agent == null) return;
            Vector3 targetPosition = agent.destination;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawSphere(targetPosition, 0.5f);
            
        }
    }
}