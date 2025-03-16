using UnityEngine;
using Obi;
using UnityEngine.UI;


public class ObiParticleController : MonoBehaviour
{
    // 粒子组信息结构体
    private struct ParticleGroupInfo
    {
        public string groupName;
        public int particleCount;
        public Vector3 centroid;
    }

    void Awake()
    {
        // 获取ObiSoftbody组件（带空值检测）
        ObiSoftbody softbody = GetComponent<ObiSoftbody>();
        if (softbody == null)
        {
            Debug.LogError("未找到ObiSoftbody组件", this);
            return;
        }

        // 验证蓝图有效性（根据[[1]][[4]][[14]]）
        if (softbody.blueprint == null)
        {
            Debug.LogWarning($"对象 {name} 未分配有效蓝图", this);
            return;
        }

        // 提取粒子组信息（根据[[1]][[4]][[6]]）
        var groups = softbody.blueprint.groups;
        if (groups.Count == 0)
        {
            Debug.Log("该软体未定义粒子组", this);
            return;
        }

        // 结构化存储信息
        ParticleGroupInfo[] groupInfos = new ParticleGroupInfo[groups.Count];

        for (int i = 0; i < groups.Count; i++)
        {
            ObiParticleGroup group = groups[i];

            // 计算粒子平均位置
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

        // 格式化输出（包含拓扑信息）
        Debug.Log($"=== 软体粒子组分析报告 ===");
        Debug.Log($"对象名称: {name}");
        Debug.Log($"蓝图类型: {softbody.blueprint.GetType().Name}");
        Debug.Log($"粒子组总数: {groups.Count}\n");

        foreach (var info in groupInfos)
        {
            Debug.Log($"[{info.groupName}] " +
                      $"粒子数: {info.particleCount} | " +
                      $"质心坐标: {info.centroid.ToString("F2")}");
        }
    }
}
