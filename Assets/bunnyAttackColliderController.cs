using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bunnyAttackColliderController : MonoBehaviour
{
    // Start is called before the first frame update

    public float attackCooldown = 0.5f; // 攻击冷却时间
    public int attackDamage = 1; // 攻击伤害
    private bool canAttack = true;
    void OnCollisionEnter(Collision collision)
    {
        if (!canAttack) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            AudioManager.Instance.Play("兔子打击到", transform.position);
            Debug.Log("兔子攻击命中玩家！");
            GameController.Instance.DeductHealth(attackDamage);

            canAttack = false;
            StartCoroutine(AttackCooldownTimer());
        }
    }

    private IEnumerator AttackCooldownTimer()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
