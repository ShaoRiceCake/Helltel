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

        [Header("Ǯ��Ԥ����")] GameObject moneyPrefab; 
        public GameObject blackFog;
        public GameObject moneybag;

        private Root behaviorTree;
        private WayPoint currentTarget;


    }
}
