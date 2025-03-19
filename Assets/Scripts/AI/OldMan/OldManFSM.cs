using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldManFSM : FSM
{
    [Header("�˺�ֵ")] public float baseDamage = 5f;
    [Header("�ȴ�ʱ��")]public float waitingTimer = 180f;
    [Header("����ʱ��")] public float happyTimer = 3f;
    [Header("�������ɼ��ʱ��")] public float duration = 0.1f;
    [Header("��������뾶")] public float baseRadius = 0.1f;
    [Header("�������Ӱ뾶")] public float increaseRadius = 0.1f;
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
                Debug.Log("��Ǯ�� ");
            }
        }
        base.ChangeState(next);
    }
}
