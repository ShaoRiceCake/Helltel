using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldManFSM : FSM
{
    [Header("伤害值")] public float baseDamage = 5f;
    [Header("等待时间")]public float waitingTimer = 180f;
    [Header("开心时间")] public float happyTimer = 3f;
    [Header("黑雾生成间隔时间")] public float duration = 0.1f;
    [Header("黑雾基础半径")] public float baseRadius = 0.1f;
    [Header("黑雾增加半径")] public float increaseRadius = 0.1f;
    public GameObject blackFog;
    public GameObject moneybag;

    public override void ChangeState(AIState next)
    {
        if (IsHost)
        {
            if(next==AIState.OldManHappy&& nameof(state) == AIState.OldManLonely.ToString())
            {
                var obj = Instantiate(moneybag, transform.position, Quaternion.identity);
                var bag = obj.GetComponent<MoneyBag>();
                bag.NetworkObject.Spawn();
                var lonely = state as OldManLonely;
                float a = 10f + lonely.waittimer * lonely.waittimer / 1000f + 0.3f * lonely.waittimer;
                bag.SetMoneyServerRpc(a);
                Debug.Log("掉钱袋 ");
            }
        }
        base.ChangeState(next);
    }
}
