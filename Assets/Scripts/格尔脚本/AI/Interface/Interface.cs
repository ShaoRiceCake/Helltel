

using UnityEngine;

public interface IHurtable
{
    void TakeDamage(int damage);
}

public interface IHealable
{
    void Heal(int amount);
}

public interface IDie
{
    bool IsDead { get; set; }
}