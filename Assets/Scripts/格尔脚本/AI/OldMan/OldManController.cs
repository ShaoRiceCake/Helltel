
using UnityEngine;
using NPBehave;
using System.Collections.Generic;

namespace Helltal.Gelercat
{
    public class OldManController : GuestBase
    {

        [Header("等待时间")] public float waitingTimer = 180f;
        [Header("开心时间")] public float happyTimer = 3f;

        [Header("孤独到死亡最小")] public float lonelyDeathMin = 120f; // 2分钟
        [Header("孤独到死亡最大")] public float lonelyDeathMax = 480f; // 8分钟

        [Header("钱币预制体")] public GameObject moneybag;

        [Header("黑雾")] public BlackForgController blackFogController; // 黑雾控制器

        [Header("触发奖励时间")] public float triggerRewardTime = 3f; // 触发奖励时间
        public LayerMask TriggerMask; // 触发器层级
        private Transform moneySpendPoint; // 钱袋生成位置 
        bool DEBUG_chatting = false;

        private Collider aroundCollider; // 触发器  玩家如果进入，就开启

        // debug

        private float lonelyDuration;  // 当前孤独状态持续时间
        private float lonelyDeathThreshold;

        protected override void Awake()
        {
            base.Awake();


            BehaviorTree = GetBehaviorTree();
#if UNITY_EDITOR
            Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
            debugger.BehaviorTree = BehaviorTree;
#endif
            BehaviorTree.Blackboard["Dead"] = false; // 死亡标志
            BehaviorTree.Blackboard["Lonely"] = false; // 孤独标志
            BehaviorTree.Blackboard["Happying"] = false; // 高兴标志
            Debug.Log("presenter:" + presenter.isActiveAndEnabled);

            moneySpendPoint = transform.Find("金币点");
        }
        protected override void Start()
        {

            base.Start();
            BehaviorTree.Start();
        }
        protected override Root GetBehaviorTree()
        {
            return new Root(
                new Selector(

                    new BlackboardCondition("Dead", Operator.IS_EQUAL, false, Stops.NONE,
                        new Selector(
                            BuildDeathBranch(),
                            BuildHappyBranch(),
                            BuildHappyingBranch(),
                            BuildLonelyBranch(),
                            BuildWaitBranch()
                        ))
                )

            );
        }
        // ===================== 状态1：等待 =====================
        private Node BuildWaitBranch()
        {
            var branch = new Sequence(
                new Action(() =>
                {
                    lonelyDuration = 0f;
                    lonelyDeathThreshold = UnityEngine.Random.Range(lonelyDeathMin, lonelyDeathMax); // 随机生成孤单阈值

                    // TODO：触发表现层动作：如坐下、张望、敲轮椅等

                }),
                new Wait(waitingTimer),
                new Action(() =>
                {

                    // 转为孤独状态
                    BehaviorTree.Blackboard["Lonely"] = true;

                })
            );

            return branch;
        }

        // ===================== 状态2：孤独 =====================
        private Node BuildLonelyBranch()
        {
            return new BlackboardCondition("Lonely", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                new Sequence(
                new Action(() =>
                    {

                        presenter.SetTrigger("ToLonelyIDLE");
                        lonelyDuration = 0f; // 重置孤独时间
                        StartBlackFog(); // 启动黑雾视觉效果
                    }
                ),
                new WaitUntilStopped()
                )

            );
        }

        // ===================== 状态3：高兴 =====================
        private Node BuildHappyBranch()
        {
            return new Condition(IsPlayerChatting, Stops.IMMEDIATE_RESTART,
                new Sequence(
                    new Action(() =>
                    {
                        if (BehaviorTree.Blackboard.Get<bool>("Lonely"))
                        {
                            presenter.SetTrigger("ToHappy");
                            // 从孤独状态进入高兴状态，触发奖励结算
                            int coins = Mathf.FloorToInt(10 + lonelyDuration * lonelyDuration / 1000f + 0.3f * lonelyDuration);
                            DropCoinBag(coins);
                            StopBlackFog();
                            BehaviorTree.Blackboard["Lonely"] = false; // 重置孤独状态
                            BehaviorTree.Blackboard["Happying"] = true; // 设置高兴状态

                        }

                    })

                )
            );
        }
        private Node BuildHappyingBranch()
        {
            return new BlackboardCondition("Happying", Operator.IS_EQUAL, true, Stops.NONE,
                new Sequence(
                    new Action(() =>
                    {
                        Debug.Log("HAPPY to wait 4");
                    }),
                    new Wait(happyTimer),
                    new Action(() =>
                    {
                        Debug.Log("HAPPY to wait 4");
                        presenter.SetTrigger("HappyToWait"); // 播放高兴动画
                        BehaviorTree.Blackboard["Happying"] = false; // 重置高兴状态
                    })
                )
            );
        }




        // ===================== 状态4：死亡 =====================
        private Node BuildDeathBranch()
        {
            return new Condition(() => BehaviorTree.Blackboard.Get<bool>("Lonely") && lonelyDuration >= lonelyDeathThreshold, Stops.IMMEDIATE_RESTART,
                    new Sequence(
                    new Action(() =>
                    {

                        TriggerDeath(); // 启动表现层的死亡动画、音效等
                        StartBlackFog2();   // 黑雾加速 & 伤害翻倍
                        BehaviorTree.Blackboard["Lonely"] = false; // 阻止状态重入                            
                        BehaviorTree.Blackboard["Dead"] = true;
                    }),
                    new WaitUntilStopped()
                    )
            );
        }





        // ===================== 实现接口 =====================



        // 判断玩家是否正在进行足够音量的聊天（true：满足条件）
        // private bool IsPlayerChatting()
        // {
        //     // TODO: 实现对指定分贝阈值的声音检测，且玩家位于老人10米内

        //     // FOR Debug:

        //     if (Debugging)
        //     {
        //         if (DEBUG_chatting)
        //         {
        //             Debug.Log("玩家正在聊天");
        //             return true;
        //         }
        //         else
        //         {
        //             return false;
        //         }
        //     }



        //     return false;
        // }
        private bool isPlayerChatting = false;  // 玩家是否满足聊天条件
        private bool hasTriggeredReward = false; // 是否已触发奖励，防止重复奖励
        private bool IsPlayerChatting()
        {
            return isPlayerChatting;
        }


        // 黑雾启动
        private void StartBlackFog()
        {

            if (debugging) Debug.Log("黑雾启动");
            blackFogController.StartFogStage1(); // 启动第一阶段黑雾
        }


        // 停止黑雾（高兴状态触发）
        private void StopBlackFog()
        {

            // blackFogController.StopFog(); // 停止黑雾
            blackFogController.FadeOutFog(); // 淡出黑雾
        }

        // 进入死亡状态的逻辑
        private void TriggerDeath()
        {
            // TODO: 播放全层音效、动画，黑雾继续扩散 & 本层熵值上限 ×1.5，触发额外客人生成
            if (debugging) Debug.Log("老人死亡");
            presenter.SetTrigger("ToDiedIDLE"); // 播放死亡动画

        }

        private void StartBlackFog2()
        {
            // TODO: 黑雾扩散速度 *3，伤害翻倍
            if (debugging) Debug.Log("黑雾加速");
            blackFogController.StartFogStage2(); // 启动第二阶段黑雾
        }

        // 掉落奖励
        private void DropCoinBag(int amount)
        {
            // TODO: 生成一个钱袋物体，金币数量为 amount

            Debug.Log("爆金币:" + amount);
            for (int i = 0; i < amount; i++)
            {
                // Instantiate(moneybag, transform.position, Quaternion.identity);

                GameObject coinBag = Instantiate(moneybag, moneySpendPoint.position, Quaternion.identity);
                MoneyPackage moneyPackage = coinBag.GetComponent<MoneyPackage>();
                if (moneyPackage != null)
                {
                    moneyPackage.Init(amount);
                }
                // 给钱袋一个随机的抛物线
                // 给钱袋一个随机的抛物线（需要 Rigidbody 组件）
                Rigidbody rb = coinBag.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 randomDir = new Vector3(
                       UnityEngine.Random.Range(-1f, 1f),     // 左右
                        1.4f,                        // 向上（保证一定抛物线）
                        UnityEngine.Random.Range(-1f, 1f)      // 前后
                    ).normalized;

                    float force = UnityEngine.Random.Range(7f, 12f); // 可调的爆炸力度
                    rb.AddForce(randomDir * force, ForceMode.Impulse);
                }
            }

        }

        // OnDestroy 时清理行为树

        private List<GameObject> playerlist;

        float playerstayTimmer = 0f; // 玩家在触发器内的时间
        protected override void Update()
        {
            base.Update();

            if (debugging)
            {
                DEBUG_chatting = Input.GetKey(KeyCode.R);


            }
            if (BehaviorTree != null)
            {
                // timer
                if (BehaviorTree.Blackboard.Get<bool>("Lonely"))
                {
                    lonelyDuration += Time.deltaTime;
                    if (lonelyDuration >= lonelyDeathThreshold)
                    {
                        // 触发死亡状态
                        BehaviorTree.Blackboard["Dead"] = true;
                    }
                }
            }


        }
        // gui 上现实lonelyDuration，方便调试
        void OnGUI()
        {
            if (debugging)
            {
                GUI.Label(new Rect(10, 10, 200, 20), "lonelyDuration: " + lonelyDuration.ToString("F2"));
                GUI.Label(new Rect(10, 30, 200, 20), "lonelyDeathThreshold: " + lonelyDeathThreshold.ToString("F2"));
            }
        }

        void OnTriggerEnter(Collider other)
        {

            // 玩家进入触发器
            playerstayTimmer = 0f; // 重置玩家在触发器内的时间

        }
        void OnTriggerExit(Collider other)
        {


            // 离开时重置状态
            playerstayTimmer = 0f;
            isPlayerChatting = false;
            hasTriggeredReward = false;

        }
        void OnTriggerStay(Collider other)
        {
            // 只有在“孤独状态”才允许判定聊天
            if (BehaviorTree.Blackboard.Get<bool>("Lonely"))
            {
                playerstayTimmer += Time.deltaTime;

                if (playerstayTimmer >= triggerRewardTime && !hasTriggeredReward)
                {
                    isPlayerChatting = true;
                    hasTriggeredReward = true; // 防止重复奖励
                }
            }
            else
            {
                // 如果不是孤独状态，就重置
                playerstayTimmer = 0f;
                isPlayerChatting = false;
                hasTriggeredReward = false;
            }
        }
        void OnDrawGizmos()
        {
            //画出触发器的范围
            if (aroundCollider != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(aroundCollider.bounds.center, aroundCollider.bounds.extents.magnitude);
            }
        }
    }
}
