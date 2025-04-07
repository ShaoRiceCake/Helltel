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
        // ͬ��transform����������
        // ��������
        [Header("�������ֵ")] public float maxHealth = 15f;
        [Header("��ǰ����ֵ")] public NetworkVariable<float> curHealth = new NetworkVariable<float>();
        [Header("��ֵ")] public float entropyValue = 0f;



        // �����ؼ�        
        public GameObject[] players;
        public Pathlist path;  // �����еĵ������б�

        public NavMeshAgent agent;  // ��������

        public GuestPresenter presenter;  // ���ֲ��api��������animator����������

        /// TODO��AudioPresident
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
                Debug.LogError("�����ڳ�������ӵ����������");
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