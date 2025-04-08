using UnityEngine;
using NPBehave;

namespace Helltal.Gelercat
{
    public class OldManController : GuestBase
    {
        [Header("�˺�ֵ")] public float baseDamage = 5f;
        [Header("�ȴ�ʱ��")] public float waitingTimer = 180f;
        [Header("����ʱ��")] public float happyTimer = 3f;
        [Header("�������ɼ��ʱ��")] public float duration = 0.1f;
        [Header("��������뾶")] public float baseRadius = 0.1f;
        [Header("�������Ӱ뾶")] public float increaseRadius = 0.1f;

        [Header("Ǯ��Ԥ����")] public GameObject moneybag;
        [Header("����")] public GameObject blackFog;

        bool DEBUG_chatting = false;

        private Root behaviorTree;



        // debug

        private float lonelyDuration;  // ��ǰ�¶�״̬����ʱ��
        private float lonelyDeathThreshold;
        protected override void Start()
        {
            base.Start();
            behaviorTree = GetBehaviorTree();
#if UNITY_EDITOR
            Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
            debugger.BehaviorTree = behaviorTree;
#endif
            behaviorTree.Start();
        }
        public override Root GetBehaviorTree()
        {
            return new Root(
                new Selector(
                    BuildDeathBranch(),
                    BuildHappyBranch(),
                    BuildLonelyBranch(),
                    BuildWaitBranch()
                )
            );
        }
        // ===================== ״̬1���ȴ� =====================
        private Node BuildWaitBranch()
        {
            var branch = new Sequence(
                new Action(() =>
                {
                    lonelyDuration = 0f;
                    lonelyDeathThreshold = UnityEngine.Random.Range(120f, 480f);
                    SetState("WAITING");
                    // TODO���������ֲ㶯���������¡������������ε�
                }),
                new Wait(waitingTimer),
                new Action(() =>
                {
                    // תΪ�¶�״̬
                    behaviorTree.Blackboard["Lonely"] = true;
                })
            );

            return branch;
        }

        // ===================== ״̬2���¶� =====================
        private Node BuildLonelyBranch()
        {
            return new BlackboardCondition("Lonely", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                new Sequence(
                    new Action(() =>
                    {
                        SetState("LONELY");
                        StartBlackFog(); // ���������Ӿ�Ч��
                    }),
                    new Service(0.1f, () =>
                    {
                        lonelyDuration += 0.1f;
                        ExpandBlackFog(0.1f); // ������չ�߼�
                    },
                        new Service(0.5f, () =>
                        {
                            ApplyFogDamage(lonelyDuration); // ÿ0.5�����һ���˺�
                        },
                            new WaitUntilStopped()
                        )
                    )
                )
            );
        }

        // ===================== ״̬3������ =====================
        private Node BuildHappyBranch()
        {
            return new Condition(IsPlayerChatting, Stops.IMMEDIATE_RESTART,
                new Sequence(
                    new Action(() =>
                    {
                        if (behaviorTree.Blackboard.Get<bool>("Lonely"))
                        {
                            // �ӹ¶�״̬�������״̬��������������
                            int coins = Mathf.FloorToInt(10 + lonelyDuration * lonelyDuration / 1000f + 0.3f * lonelyDuration);
                            DropCoinBag(coins);
                            StopBlackFog();
                            // ���ñ�־
                            behaviorTree.Blackboard["Lonely"] = false; // ��ֹ״̬����
                        }
                        SetState("HAPPY");
                    }),
                    new Wait(3f),
                    new Action(() =>
                    {
                        if (!IsPlayerChatting())
                        {
                            SetState("WAITING"); // ���صȴ�״̬
                        }
                    })
                )
            );
        }

        // ===================== ״̬4������ =====================
        private Node BuildDeathBranch()
        {
            return new Condition(() => behaviorTree.Blackboard.Get<bool>("Lonely") && lonelyDuration >= lonelyDeathThreshold,
                new Action(() =>
                {
                    SetState("DEAD");
                    TriggerDeath(); // �������ֲ��������������Ч��
                    AmplifyFog();   // ������� & �˺�����
                    behaviorTree.Blackboard["Lonely"] = false; // ��ֹ״̬����
                })
            );
        }

        // ===================== ʵ�ֽӿ� =====================

        // �������˵�ǰ״̬��WAITING / LONELY / HAPPY / DEAD��
        private void SetState(string state)
        {

            if (Debugging) Debug.Log("����״̬��Ϊ��" + state);
            // TODO: ״̬�л�ʱ�������ֲ㶯��/��Ч
        }

        // �ж�����Ƿ����ڽ����㹻���������죨true������������
        private bool IsPlayerChatting()
        {
            // TODO: ʵ�ֶ�ָ���ֱ���ֵ��������⣬�����λ������10����

            // FOR Debug:

            if (Debugging){
                if (DEBUG_chatting)
                {
                    Debug.Log("�����������");
                    return true;
                }
                else
                {
                    return false;
                }
            }



            return false;
        }

        // ��������
        private void StartBlackFog()
        {
            // TODO: ֪ͨ���ֲ����ɺ�����Ч�����ó�ʼ�뾶
            if (Debugging) Debug.Log("��������");
        }

        // ���������ɢ�߼�
        private void ExpandBlackFog(float deltaTime)
        {
            // TODO: ÿ0.1���������뾶���������� b��
            if (Debugging) Debug.Log("������ɢ,deltaTime:" + deltaTime);
        }

        // ������������˺�
        private void ApplyFogDamage(float timeLonely)
        {
            // TODO: ÿ0.5�����һ���˺����˺� = ����ֵ + t����*c��
            if (Debugging) Debug.Log("�����˺�,ʱ��:" + timeLonely);
        }

        // ֹͣ��������״̬������
        private void StopBlackFog()
        {
            // TODO: ʹ����������
            if (Debugging) Debug.Log("����ֹͣ");
        }

        // ��������״̬���߼�
        private void TriggerDeath()
        {
            // TODO: ����ȫ����Ч�����������������ɢ & ������ֵ���� ��1.5�����������������
            if (Debugging) Debug.Log("��������");
        }

        // ���ٺ�������״̬�£�
        private void AmplifyFog()
        {
            // TODO: ������ɢ�ٶ� *3���˺�����
            if (Debugging) Debug.Log("�������");
        }

        // ���佱��
        private void DropCoinBag(int amount)
        {
            // TODO: ����һ��Ǯ�����壬�������Ϊ amount

            GameObject coinBag = Instantiate(moneybag, transform.position, Quaternion.identity);
            MoneyPackage moneyPackage = coinBag.GetComponent<MoneyPackage>();
            if (moneyPackage != null)
            {
                moneyPackage.Init(amount);
            }
            // ��Ǯ��һ�������������
            // ��Ǯ��һ������������ߣ���Ҫ Rigidbody �����
            Rigidbody rb = coinBag.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 randomDir = new Vector3(
                   UnityEngine.Random.Range(-1f, 1f),     // ����
                    1f,                        // ���ϣ���֤һ�������ߣ�
                    UnityEngine.Random.Range(-1f, 1f)      // ǰ��
                ).normalized;

                float force = UnityEngine.Random.Range(4f, 7f); // �ɵ��ı�ը����
                rb.AddForce(randomDir * force, ForceMode.Impulse);
            }

        }

        // OnDestroy ʱ������Ϊ��
        public override void OnDestroy()
        {
            if (behaviorTree != null && behaviorTree.CurrentState == Node.State.ACTIVE)
            {
                behaviorTree.Stop();
            }
        }


        void Update()
        {
            if (Debugging)
            {
                DEBUG_chatting = Input.GetKey(KeyCode.R);
            }
        }
    }
}
