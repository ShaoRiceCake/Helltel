using System.Collections;
using UnityEngine;

public class PlayAndDestroyEffect : MonoBehaviour
{
    public ParticleSystem particle;

    void Start()
    {
        if (particle == null)
            particle = GetComponent<ParticleSystem>();
    
        StartCoroutine(WaitForEnd());
    }

    IEnumerator WaitForEnd()
    {
        // 等待粒子系统播放完成
        yield return new WaitUntil(() => !particle.IsAlive(true));
        Destroy(gameObject);
    }
}
