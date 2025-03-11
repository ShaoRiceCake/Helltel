using UnityEngine;
using Obi;
using UnityEngine.UI;


public class ObiParticleController : MonoBehaviour
{
    // ��������Ϣ�ṹ��
    private struct ParticleGroupInfo
    {
        public string groupName;
        public int particleCount;
        public Vector3 centroid;
    }

    void Awake()
    {
        // ��ȡObiSoftbody���������ֵ��⣩
        ObiSoftbody softbody = GetComponent<ObiSoftbody>();
        if (softbody == null)
        {
            Debug.LogError("δ�ҵ�ObiSoftbody���", this);
            return;
        }

        // ��֤��ͼ��Ч�ԣ�����[[1]][[4]][[14]]��
        if (softbody.blueprint == null)
        {
            Debug.LogWarning($"���� {name} δ������Ч��ͼ", this);
            return;
        }

        // ��ȡ��������Ϣ������[[1]][[4]][[6]]��
        var groups = softbody.blueprint.groups;
        if (groups.Count == 0)
        {
            Debug.Log("������δ����������", this);
            return;
        }

        // �ṹ���洢��Ϣ
        ParticleGroupInfo[] groupInfos = new ParticleGroupInfo[groups.Count];

        for (int i = 0; i < groups.Count; i++)
        {
            ObiParticleGroup group = groups[i];

            // ��������ƽ��λ��
            Vector3 centroid = Vector3.zero;
            foreach (int index in group.particleIndices)
            {
                centroid += softbody.GetParticlePosition(index);
            }
            centroid /= group.particleIndices.Count;

            groupInfos[i] = new ParticleGroupInfo
            {
                groupName = group.name,
                particleCount = group.particleIndices.Count,
                centroid = centroid
            };
        }

        // ��ʽ�����������������Ϣ��
        Debug.Log($"=== ����������������� ===");
        Debug.Log($"��������: {name}");
        Debug.Log($"��ͼ����: {softbody.blueprint.GetType().Name}");
        Debug.Log($"����������: {groups.Count}\n");

        foreach (var info in groupInfos)
        {
            Debug.Log($"[{info.groupName}] " +
                      $"������: {info.particleCount} | " +
                      $"��������: {info.centroid.ToString("F2")}");
        }
    }
}
