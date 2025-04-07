using UnityEngine;
using NPBehave;

namespace Helltal.Gelercat
{
    public class OldManController : GuestBase
    {
        [Header("伤害值")] public float baseDamage = 5f;
        [Header("等待时间")] public float waitingTimer = 180f;
        [Header("开心时间")] public float happyTimer = 3f;
        [Header("黑雾生成间隔时间")] public float duration = 0.1f;
        [Header("黑雾基础半径")] public float baseRadius = 0.1f;
        [Header("黑雾增加半径")] public float increaseRadius = 0.1f;

        [Header("钱币预制体")] GameObject moneyPrefab; 
        public GameObject blackFog;
        public GameObject moneybag;

        private Root behaviorTree;
        private WayPoint currentTarget;


    }
}
