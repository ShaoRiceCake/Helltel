using UnityEngine;
using NPBehave;

namespace Helltal.Gelercat
{
    public class OldManController : GuestBase
    {

        [Header("等待时间")] public float waitingTimer = 180f;
        [Header("开心时间")] public float happyTimer = 3f;
        [Header("黑雾生成间隔时间")] public float duration = 0.1f;

        [Header("孤独到死亡最小")] public float lonelyDeathMin = 120f; // 2分钟
        [Header("孤独到死亡最大")] public float lonelyDeathMax = 480f; // 8分钟

        [Header("钱币预制体")] public GameObject moneybag;


        bool DEBUG_chatting = false;

        private Root behaviorTree;



        // debug

        private float lonelyDuration;  // 当前孤独状态持续时间
        private float lonelyDeathThreshold;


        protected override void Start()
        {
            base.Start();
            behaviorTree = GetBehaviorTree();
#if UNITY_EDITOR
            Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
            debugger.BehaviorTree = behaviorTree;
#endif
            behaviorTree.Blackboard["Dead"] = false; // 死亡标志
            behaviorTree.Blackboard["Lonely"] = false; // 孤独标志
            behaviorTree.Start();
        }
        public override Root GetBehaviorTree()
        {
            return new Root(
                new Selector(

                    new BlackboardCondition("Dead", Operator.IS_EQUAL, false, Stops.NONE,
                        new Selector(
                            BuildDeathBranch(),
                            BuildHappyBranch(),
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
                    lonelyDeathThreshold = UnityEngine.Random.Range(lonelyDeathMin, lonelyDeathMax); // 随机生成死亡阈值
                    SetState("WAITING");
                    // TODO：触发表现层动作：如坐下、张望、敲轮椅等
                }),
                new Wait(waitingTimer),
                new Action(() =>
                {
                    // 转为孤独状态
                    behaviorTree.Blackboard["Lonely"] = true;
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
                        SetState("LONELY");
                        StartBlackFog(); // 启动黑雾视觉效果
                    }),
                    new Service(duration, () =>
                        {
                            lonelyDuration += duration; // 增加孤独状态持续时间
                            ExpandBlackFog(duration); // 黑雾扩展逻辑
                        },
                    //     new Service(0.5f, () =>
                    //     {
                    //         ApplyFogDamage(lonelyDuration); // 每0.5秒造成一次伤害
                    //     },
                         // 伤害移动到黑雾逻辑中
                    //  )
                    new WaitUntilStopped()
                    )
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
                        if (behaviorTree.Blackboard.Get<bool>("Lonely"))
                        {
                            // 从孤独状态进入高兴状态，触发奖励结算
                            int coins = Mathf.FloorToInt(10 + lonelyDuration * lonelyDuration / 1000f + 0.3f * lonelyDuration);
                            DropCoinBag(coins);
                            StopBlackFog();
                            // 重置标志
                            behaviorTree.Blackboard["Lonely"] = false; // 阻止状态重入
                        }
                        SetState("HAPPY");
                    }),
                    new Wait(3f),
                    new Action(() =>
                    {
                        if (!IsPlayerChatting())
                        {
                            SetState("WAITING"); // 返回等待状态
                        }
                    })
                )
            );
        }

        // ===================== 状态4：死亡 =====================
        private Node BuildDeathBranch()
        {
            return new Condition(() => behaviorTree.Blackboard.Get<bool>("Lonely") && lonelyDuration >= lonelyDeathThreshold, Stops.IMMEDIATE_RESTART,

                new Sequence(               
                    new Action(() =>
                    {
                        SetState("DEAD");
                        TriggerDeath(); // 启动表现层的死亡动画、音效等
                        AmplifyFog();   // 黑雾加速 & 伤害翻倍
                        behaviorTree.Blackboard["Lonely"] = false; // 阻止状态重入                            
                        behaviorTree.Blackboard["Dead"] = true;
                    })
                    // new Service()
                )
            );
        }

        // ===================== 状态5：死亡后 黑雾扩散 =====================



        // ===================== 实现接口 =====================

        // 设置老人当前状态（WAITING / LONELY / HAPPY / DEAD）
        private void SetState(string state)
        {

            if (Debugging) Debug.Log("老人状态变为：" + state);
            // TODO: 状态切换时触发表现层动画/音效
        }

        // 判断玩家是否正在进行足够音量的聊天（true：满足条件）
        private bool IsPlayerChatting()
        {
            // TODO: 实现对指定分贝阈值的声音检测，且玩家位于老人10米内

            // FOR Debug:

            if (Debugging)
            {
                if (DEBUG_chatting)
                {
                    Debug.Log("玩家正在聊天");
                    return true;
                }
                else
                {
                    return false;
                }
            }



            return false;
        }

        // 黑雾启动
        private void StartBlackFog()
        {
            // TODO: 通知表现层生成黑雾特效，设置初始半径
            if (Debugging) Debug.Log("黑雾启动");
        }

        // 黑雾持续扩散逻辑
        private void ExpandBlackFog(float deltaTime)
        {
            // TODO: 每0.1秒扩大黑雾半径（增长速率 b）
            if( Debugging) Debug.Log("黑雾扩散,deltaTime:" + deltaTime);
            // if (Debugging) Debug.Log("黑雾扩散,deltaTime:" + deltaTime);
        }

        // 黑雾对玩家造成伤害
        private void ApplyFogDamage(float time)
        {
            // TODO: 每0.5秒造成一次伤害（伤害 = 基础值 + t分钟*c）
            if (Debugging) Debug.Log("黑雾伤害,时间:" );
        }

        // 停止黑雾（高兴状态触发）
        private void StopBlackFog()
        {
            // TODO: 使黑雾逐渐消退
            if (Debugging) Debug.Log("黑雾停止");
        }

        // 进入死亡状态的逻辑
        private void TriggerDeath()
        {
            // TODO: 播放全层音效、动画，黑雾继续扩散 & 本层熵值上限 ×1.5，触发额外客人生成
            if (Debugging) Debug.Log("老人死亡");


        }

        // 加速黑雾（死亡状态下）
        private void AmplifyFog()
        {
            // TODO: 黑雾扩散速度 *3，伤害翻倍
            if (Debugging) Debug.Log("黑雾加速");
        }

        // 掉落奖励
        private void DropCoinBag(int amount)
        {
            // TODO: 生成一个钱袋物体，金币数量为 amount

            Debug.Log("爆金币:" + amount);
            GameObject coinBag = Instantiate(moneybag, transform.position, Quaternion.identity);
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
                    1f,                        // 向上（保证一定抛物线）
                    UnityEngine.Random.Range(-1f, 1f)      // 前后
                ).normalized;

                float force = UnityEngine.Random.Range(4f, 7f); // 可调的爆炸力度
                rb.AddForce(randomDir * force, ForceMode.Impulse);
            }

        }

        // OnDestroy 时清理行为树
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
                Debug.Log("lonelyDuration:" + lonelyDuration);

            }

            // timer


        }
    }
}
