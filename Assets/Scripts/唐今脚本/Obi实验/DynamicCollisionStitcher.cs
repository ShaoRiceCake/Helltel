// using UnityEngine;
// using Obi;
// using System.Collections.Generic;
// using System; // 需要引入 System 命名空间以使用 ValueTuple
//
// /// <summary>
// /// (新功能版)
// /// 响应碰撞，动态地在被触碰的软体粒子及其邻近粒子之间创建临时的“缝合(Stitch)”约束。
// /// 效果：当被标记物体碰撞时，软体接触区域会瞬间“变硬”或“结晶”。
// /// 核心：使用 ObiStitchConstraints 并沿用原脚本高效的 "Swap and Pop" 池管理技术。
// /// </summary>
// [RequireComponent(typeof(ObiSoftbody))]
// public class DynamicCollisionStitcher : MonoBehaviour
// {
//     [Header("核心触发配置")]
//     [Tooltip("只响应拥有此Tag的物体的碰撞。")]
//     public string triggerColliderTag = "Stiffener";
//     [Tooltip("预分配的 Stitch 约束池大小。这代表能同时存在的粒子连接对的最大数量。")]
//     public int stitchPoolSize = 256;
//
//     [Header("缝合效果配置")]
//     [Tooltip("缝合约束的硬度 (0-1)。值越高，粒子间的连接越刚硬。")]
//     [Range(0f, 1f)]
//     public float stitchStiffness = 1f;
//     [Tooltip("从碰撞点开始搜索邻近粒子的半径。半径越大，受影响的区域越广。")]
//     public float searchRadius = 0.5f;
//     [Tooltip("每个缝合约束的持续时间（秒）。超时后会自动解除。")]
//     public float stitchLifetime = 2.0f;
//
//
//     [Header("可视化与调试")]
//     public bool enableVisualization = true;
//     public Color stitchedParticleColor = Color.red;
//
//     // C# 层的状态追踪器
//     private class StitchInfo { public float CreationTime; }
//     private StitchInfo[] stitchInfos;
//
//     private ObiSoftbody softbody;
//     private ObiSolver solver;
//     private ObiStitchConstraintsData stitchConstraintsData;
//     private ObiStitchConstraintsBatch dynamicStitchBatch;
//
//     // 用于快速查找一个约束对是否已存在，防止重复创建
//     // 使用 ValueTuple<int, int> 作为 Key，其中 item1 始终是较小的粒子索引
//     private HashSet<ValueTuple<int, int>> existingStitchPairs = new HashSet<ValueTuple<int, int>>();
//     
//     // 用于追踪哪些粒子被激活了，方便染色
//     private Dictionary<int, int> activeParticleStitchCount = new Dictionary<int, int>();
//     private readonly Dictionary<int, Color> originalParticleColors = new Dictionary<int, Color>();
//
//     void OnEnable() { softbody = GetComponent<ObiSoftbody>(); softbody.OnBlueprintLoaded += OnBlueprintLoaded; if (softbody.isLoaded) OnBlueprintLoaded(softbody, softbody.sourceBlueprint); }
//     void OnDisable() { softbody.OnBlueprintLoaded -= OnBlueprintLoaded; RemoveDynamicBatch(); RestoreAllParticleColors(); }
//     private void OnBlueprintLoaded(ObiActor actor, ObiActorBlueprint blueprint) { SetupDynamicBatch(); SubscribeToSolver(); }
//     private void SubscribeToSolver() { if (solver != null) solver.OnCollision -= Solver_OnCollision; solver = softbody.solver; if (solver != null) solver.OnCollision += Solver_OnCollision; }
//
//     private void SetupDynamicBatch()
//     {
//         RemoveDynamicBatch();
//         // **核心改变 1: 获取 Stitch 约束组件**
//         stitchConstraintsData = softbody.GetConstraintsByType(Oni.ConstraintType.Stitch) as ObiStitchConstraintsData;
//         if (stitchConstraintsData == null) { Debug.LogError($"[{this.name}] ObiSoftbody上缺少StitchConstraints组件。请添加一个Obi Stitch Constraints。", this); enabled = false; return; }
//
//         dynamicStitchBatch = new ObiStitchConstraintsBatch();
//         stitchInfos = new StitchInfo[stitchPoolSize];
//         for (int i = 0; i < stitchPoolSize; ++i)
//         {
//             // 为 Stitch 约束池预分配内存，参数为 (粒子A, 粒子B, 初始长度, 强度, 容差)
//             dynamicStitchBatch.AddConstraint(-1, -1, 0, 1, 1);
//             stitchInfos[i] = new StitchInfo();
//         }
//
//         dynamicStitchBatch.activeConstraintCount = 0;
//         stitchConstraintsData.AddBatch(dynamicStitchBatch);
//     }
//
//     private void RemoveDynamicBatch()
//     {
//         if (solver != null) solver.OnCollision -= Solver_OnCollision;
//         solver = null;
//
//         if (stitchConstraintsData != null && dynamicStitchBatch != null)
//         {
//             stitchConstraintsData.RemoveBatch(dynamicStitchBatch);
//             if (softbody.isLoaded) softbody.SetConstraintsDirty(Oni.ConstraintType.Stitch);
//         }
//         dynamicStitchBatch = null;
//         stitchConstraintsData = null;
//         existingStitchPairs.Clear();
//         activeParticleStitchCount.Clear();
//     }
//
//     void LateUpdate()
//     {
//         if (!softbody.isLoaded || dynamicStitchBatch == null || stitchLifetime <= 0) return;
//
//         bool needsUpdate = false;
//         // 倒序遍历以安全地在循环中移除元素
//         for (int i = dynamicStitchBatch.activeConstraintCount - 1; i >= 0; --i)
//         {
//             // **核心改变 2: 基于生命周期的失效逻辑**
//             if (Time.time - stitchInfos[i].CreationTime > stitchLifetime)
//             {
//                 DeactivateStitch(i);
//                 needsUpdate = true;
//             }
//         }
//
//         if (needsUpdate)
//         {
//             softbody.SetConstraintsDirty(Oni.ConstraintType.Stitch);
//             UpdateColors();
//         }
//     }
//
//     private void Solver_OnCollision(ObiSolver solver, ObiNativeContactList contacts)
//     {
//         if (contacts.count == 0 || dynamicStitchBatch == null) return;
//
//         bool needsUpdate = false;
//
//         // 使用一个HashSet来确保在同一帧内只处理每个被碰撞的粒子一次
//         HashSet<int> processedParticlesThisFrame = new HashSet<int>();
//
//         for (int i = 0; i < contacts.count; ++i)
//         {
//             // 池已满，停止处理
//             if (dynamicStitchBatch.activeConstraintCount >= stitchPoolSize)
//             {
//                 //Debug.LogWarning("Stitch约束池已满!");
//                 break;
//             }
//
//             Oni.Contact contact = contacts[i];
//             int particleSolverIndex = GetParticleSolverIndexFromContact(contact);
//             if (particleSolverIndex == -1 || processedParticlesThisFrame.Contains(particleSolverIndex)) continue;
//
//             var otherCollider = ObiColliderWorld.GetInstance().colliderHandles[GetColliderIndexFromContact(contact)].owner;
//             if (otherCollider == null || !otherCollider.gameObject.activeInHierarchy) continue;
//             if (!string.IsNullOrEmpty(triggerColliderTag) && !otherCollider.CompareTag(triggerColliderTag)) continue;
//
//             // **核心改变 3: 查找邻近粒子并创建Stitch**
//             // 标记此粒子已处理
//             processedParticlesThisFrame.Add(particleSolverIndex);
//
//             // 使用 Obi Solver 内置的加速结构查找邻居
//             var neighbours = new ObiNativeIntList();
//             solver.GetNeighbouringParticles(particleSolverIndex, searchRadius, neighbours);
//
//             foreach (int neighbourIndex in neighbours)
//             {
//                 // 确保有空间
//                 if (dynamicStitchBatch.activeConstraintCount >= stitchPoolSize) break;
//                 // 不和自己创建约束
//                 if (particleSolverIndex == neighbourIndex) continue;
//
//                 // 创建约束
//                 needsUpdate |= ActivateStitch(particleSolverIndex, neighbourIndex);
//             }
//             neighbours.Dispose(); // 必须释放 ObiNativeList
//         }
//
//         if (needsUpdate)
//         {
//             softbody.SetConstraintsDirty(Oni.ConstraintType.Stitch);
//             UpdateColors();
//         }
//     }
//
//     private bool ActivateStitch(int particleA, int particleB)
//     {
//         // 确保A总是较小的索引，以方便去重
//         if (particleA > particleB) { var temp = particleA; particleA = particleB; particleB = temp; }
//
//         var stitchPair = new ValueTuple<int, int>(particleA, particleB);
//         
//         // 如果这个连接对已存在，则不创建
//         if (existingStitchPairs.Contains(stitchPair)) return false;
//
//         int slotIndex = dynamicStitchBatch.activeConstraintCount;
//
//         // 填充 Stitch 批处理数据
//         // Stitch 的粒子索引是成对存储的
//         dynamicStitchBatch.particleIndices[slotIndex * 2] = particleA;
//         dynamicStitchBatch.particleIndices[slotIndex * 2 + 1] = particleB;
//
//         // 设置约束的静止长度为它们当前在空间中的距离
//         float restLength = Vector3.Distance(solver.positions[particleA], solver.positions[particleB]);
//         dynamicStitchBatch.restLengths[slotIndex] = restLength;
//         
//         // 设置硬度和容差
//         dynamicStitchBatch.stiffnesses[slotIndex * 2] = 1f - stitchStiffness;
//         dynamicStitchBatch.stiffnesses[slotIndex * 2 + 1] = 1f;
//
//         // 更新 C# 追踪信息
//         stitchInfos[slotIndex].CreationTime = Time.time;
//         
//         // 更新追踪数据
//         existingStitchPairs.Add(stitchPair);
//         IncrementParticleStitchCount(particleA);
//         IncrementParticleStitchCount(particleB);
//
//         dynamicStitchBatch.activeConstraintCount++;
//         return true;
//     }
//
//     private void DeactivateStitch(int slotIndex)
//     {
//         // 1. 从追踪集合中移除要停用的约束对
//         int particleA = dynamicStitchBatch.particleIndices[slotIndex * 2];
//         int particleB = dynamicStitchBatch.particleIndices[slotIndex * 2 + 1];
//         // 确保索引顺序正确以匹配key
//         if (particleA > particleB) { var temp = particleA; particleA = particleB; particleB = temp; }
//         existingStitchPairs.Remove(new ValueTuple<int, int>(particleA, particleB));
//
//         // 减少粒子激活计数
//         DecrementParticleStitchCount(particleA);
//         DecrementParticleStitchCount(particleB);
//
//         // 2. "Swap and Pop" 核心逻辑
//         dynamicStitchBatch.activeConstraintCount--;
//         int lastActiveIndex = dynamicStitchBatch.activeConstraintCount;
//
//         if (slotIndex < lastActiveIndex)
//         {
//             // 复制最后一个元素的数据到当前槽位
//             dynamicStitchBatch.particleIndices[slotIndex * 2] = dynamicStitchBatch.particleIndices[lastActiveIndex * 2];
//             dynamicStitchBatch.particleIndices[slotIndex * 2 + 1] = dynamicStitchBatch.particleIndices[lastActiveIndex * 2 + 1];
//             dynamicStitchBatch.restLengths[slotIndex] = dynamicStitchBatch.restLengths[lastActiveIndex];
//             dynamicStitchBatch.stiffnesses[slotIndex * 2] = dynamicStitchBatch.stiffnesses[lastActiveIndex * 2];
//             dynamicStitchBatch.stiffnesses[slotIndex * 2 + 1] = dynamicStitchBatch.stiffnesses[lastActiveIndex * 2 + 1];
//             
//             // 复制C#追踪信息
//             stitchInfos[slotIndex].CreationTime = stitchInfos[lastActiveIndex].CreationTime;
//
//             // 4. 更新被移动的约束对在集合中的key (先移除旧的，再添加新的)
//             int movedA = dynamicStitchBatch.particleIndices[slotIndex * 2];
//             int movedB = dynamicStitchBatch.particleIndices[slotIndex * 2 + 1];
//             if (movedA > movedB) { var temp = movedA; movedA = movedB; movedB = temp; }
//             
//             // 重要：必须先从集合里移除最后一个元素的旧记录，因为它现在要被停用了
//             int oldLastA = dynamicStitchBatch.particleIndices[lastActiveIndex * 2];
//             int oldLastB = dynamicStitchBatch.particleIndices[lastActiveIndex * 2 + 1];
//             if (oldLastA > oldLastB) { var temp = oldLastA; oldLastA = oldLastB; oldLastB = temp; }
//             existingStitchPairs.Remove(new ValueTuple<int, int>(oldLastA, oldLastB));
//             
//             // 添加被移动到新位置的约束对
//             existingStitchPairs.Add(new ValueTuple<int, int>(movedA, movedB));
//         }
//     }
//
//     // --- 颜色和辅助方法 ---
//     private void IncrementParticleStitchCount(int particleIndex) { if (!activeParticleStitchCount.ContainsKey(particleIndex)) activeParticleStitchCount[particleIndex] = 0; activeParticleStitchCount[particleIndex]++; }
//     private void DecrementParticleStitchCount(int particleIndex) { if (activeParticleStitchCount.ContainsKey(particleIndex)) { activeParticleStitchCount[particleIndex]--; if (activeParticleStitchCount[particleIndex] <= 0) activeParticleStitchCount.Remove(particleIndex); } }
//     
//     private void UpdateColors()
//     {
//         if (!enableVisualization || solver == null) return;
//         RestoreAllParticleColors();
//         foreach (var particleIndex in activeParticleStitchCount.Keys)
//         {
//             if (!originalParticleColors.ContainsKey(particleIndex))
//             {
//                 originalParticleColors[particleIndex] = solver.colors[particleIndex];
//             }
//             solver.colors[particleIndex] = stitchedParticleColor;
//         }
//         solver.colors.Upload();
//     }
//
//     private void RestoreAllParticleColors()
//     {
//         if (originalParticleColors.Count > 0 && solver != null)
//         {
//             foreach (var p in originalParticleColors)
//             {
//                 if (p.Key >= 0 && p.Key < solver.colors.count) solver.colors[p.Key] = p.Value;
//             }
//             solver.colors.Upload();
//         }
//         originalParticleColors.Clear();
//     }
//     
//     // 辅助函数 (与原脚本相同)
//     private int GetParticleSolverIndexFromContact(Oni.Contact contact) { if(IsParticleFromOurSoftbody(contact.bodyA)) return contact.bodyA; if(IsParticleFromOurSoftbody(contact.bodyB)) return contact.bodyB; return -1; }
//     private int GetColliderIndexFromContact(Oni.Contact contact) { return IsParticleFromOurSoftbody(contact.bodyA) ? contact.bodyB : contact.bodyA; }
//     private bool IsParticleFromOurSoftbody(int particleSolverIndex) { if(solver == null || particleSolverIndex < 0 || particleSolverIndex >= solver.particleToActor.Length) return false; var p = solver.particleToActor[particleSolverIndex]; return p != null && p.actor == softbody; }
// }