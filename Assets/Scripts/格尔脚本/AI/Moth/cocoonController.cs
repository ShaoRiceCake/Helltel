using System.Collections;
using System.Collections.Generic;
using Helltal.Gelercat;
using UnityEngine;

/// <summary>
/// 羽衣蛾的茧控制器（cocoon）
/// </summary>
public class cocoonController : MonoBehaviour
{
    [Header("羽衣蛾预制体")] public GameObject mothPrefab;
    [Header("触发后孵卵时间范围")] public Vector2 hatchTimeRange = new Vector2(0.2f, 0.8f);
    [Header("所属虫群")] public MothGroupController mothGroup;
    [Header("连锁范围")] public float chainDistance = 5f;
    [Header("孵化出几只")] public int hatchCount = 1;

    private MaterialFadeAndDestroy materialController;
    public Animator animator;

    private bool hatchlock = false;

    // 全局防重复连锁

    void Awake()
    {
        var collider = this.gameObject.GetComponent<MeshCollider>();
        if (collider == null)
        {
            collider = this.gameObject.AddComponent<MeshCollider>();
        }
        collider.convex = true;
        collider.isTrigger = true; // ✅ 使用 Trigger 检测
        materialController = GetComponent<MaterialFadeAndDestroy>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("PlayerBodyItem")) && !hatchlock)
        {
            Debug.Log("Player Enter");
            TriggerHatch();
        }
    }

    public void TriggerHatch()
    {
        if (hatchlock) return; // 防止重复触发
        hatchlock = true;

        // 播放孵化动画
        animator.SetTrigger("Hatch");
        // 在动画回调中调用 OnHatchEnd
        StartCoroutine(HatchCoroutine());
    }


    private IEnumerator HatchCoroutine()
    {
        float delay = Random.Range(hatchTimeRange.x, hatchTimeRange.y);
        yield return new WaitForSeconds(delay);

        // 实例化蛾子
        if (mothPrefab != null)
        {
            for (int i = 0; i < hatchCount; i++)
            {
                Vector3 offset = Random.insideUnitSphere * 0.5f; // 随机偏移
                GameObject mothGO = Instantiate(mothPrefab, transform.position+offset, Quaternion.identity);
                MothController moth = mothGO.GetComponent<MothController>();
                if (moth != null)
                {
                    moth.Init(mothGroup);
                }
            }
        }

        // 触发范围内的其他 cocoon
        Collider[] hits = Physics.OverlapSphere(transform.position, chainDistance);
        foreach (var hit in hits)
        {
            if (hit.gameObject == this.gameObject) continue;
            cocoonController otherCocoon = hit.GetComponent<cocoonController>();
            if (otherCocoon != null)
            {
                otherCocoon.TriggerHatch(); // 递归触发（防止重复由 HashSet 控制）
            }
        }

        // 动画播放完毕后销毁本体
        yield return new WaitForSeconds(1.5f); // 确保动画完成
        materialController.StartFadeAndDestroy();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chainDistance);
    }
}
