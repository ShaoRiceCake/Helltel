using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallonCollisionController : MonoBehaviour
{
    // Start is called before the first frame update
    public BalloonController thisballon;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("PlayerBodyItem"))
        {
            // 处理与玩家身体物品的碰撞
            // 例如，触发气球的爆炸效果
            Debug.Log("气球与玩家身体物品碰撞！");
            thisballon.TakeDamage(thisballon.maxHealth);
        }

    }

    
}
