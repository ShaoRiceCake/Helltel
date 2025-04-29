using UnityEngine;

public class MonsterHurtEvent
{
    public int monsterID;       // 怪物ID
    public Vector3 hurtPosition; // 受伤位置（世界坐标）
    public float damage;        // 伤害值
    public Transform monsterTransform; // 怪物Transform（可选）

    public MonsterHurtEvent(int monsterID, Vector3 hurtPosition, float damage, Transform monsterTransform = null)
    {
        this.monsterID = monsterID;
        this.hurtPosition = hurtPosition;
        this.damage = damage;
        this.monsterTransform = monsterTransform;
    }
}