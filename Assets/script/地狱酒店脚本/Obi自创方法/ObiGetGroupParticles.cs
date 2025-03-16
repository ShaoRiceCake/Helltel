using UnityEngine;
using Obi;
using System.Collections.Generic;

public static class ObiGetGroupParticles
{
    /// <summary>
    /// 获取 ObiParticleAttachment 中所有粒子在 Solver 中的索引集合。
    /// </summary>
    /// <param name="attachment">ObiParticleAttachment 组件</param>
    /// <returns>粒子在 Solver 中的索引集合</returns>
    public static List<int> GetParticleSolverIndices(ObiParticleAttachment attachment)
    {
        List<int> solverIndices = new List<int>();

        if (attachment == null || attachment.particleGroup == null)
        {
            Debug.LogError("ObiParticleAttachment or ParticleGroup is null.");
            return solverIndices;
        }

        ObiActor actor = attachment.GetComponent<ObiActor>();
        if (actor == null)
        {
            Debug.LogError("Failed to get ObiActor from ObiParticleAttachment.");
            return solverIndices;
        }

        ObiSolver solver = actor.solver;
        if (solver == null)
        {
            Debug.LogError("Failed to get ObiSolver from ObiActor.");
            return solverIndices;
        }

        // 遍历粒子组中的粒子
        foreach (int particleIndex in attachment.particleGroup.particleIndices)
        {
            if (particleIndex >= 0 && particleIndex < actor.solverIndices.count)
            {
                int solverIndex = actor.solverIndices[particleIndex];
                if (solverIndex >= 0 && solverIndex < solver.positions.count)
                {
                    solverIndices.Add(solverIndex);
                }
                else
                {
                    Debug.LogWarning($"Solver index {solverIndex} is out of range.");
                }
            }
            else
            {
                Debug.LogWarning($"Particle index {particleIndex} is out of range.");
            }
        }

        if (solverIndices == null)
        {
            Debug.LogError("Get solverIndices fall!");
        }

        return solverIndices;
    }

    /// <summary>
    /// 获取 ObiParticleAttachment 中所有粒子的世界坐标。
    /// </summary>
    /// <param name="attachment">ObiParticleAttachment 组件</param>
    /// <returns>粒子的世界坐标列表</returns>
    public static List<Vector3> GetParticleWorldPositions(ObiParticleAttachment attachment)
    {
        List<Vector3> worldPositions = new List<Vector3>();

        if (attachment == null || attachment.particleGroup == null)
        {
            Debug.LogError("ObiParticleAttachment or ParticleGroup is null.");
            return worldPositions;
        }

        ObiActor actor = attachment.GetComponent<ObiActor>();
        ObiSolver solver = actor?.solver;

        if (actor == null || solver == null)
        {
            Debug.LogError("Failed to get ObiActor or ObiSolver.");
            return worldPositions;
        }

        // 遍历粒子组中的粒子
        foreach (int particleIndex in attachment.particleGroup.particleIndices)
        {
            if (particleIndex >= 0 && particleIndex < actor.solverIndices.count)
            {
                int solverIndex = actor.solverIndices[particleIndex];
                if (solverIndex >= 0 && solverIndex < solver.positions.count)
                {
                    // 将局部坐标转换为世界坐标
                    Vector3 localPosition = solver.positions[solverIndex];
                    Vector3 worldPosition = solver.transform.localToWorldMatrix.MultiplyPoint3x4(localPosition);
                    worldPositions.Add(worldPosition);
                }
            }
        }

        return worldPositions;
    }
}