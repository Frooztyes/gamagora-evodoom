using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LivingCreature : ScriptableObject
{
    [Header("Health")]
    public float MaxHealth = 100f;
    public float Health = 100f;

    [Header("Movements")]
    public float MoveSpeed = 20f;
    public float JumpForce = 80f;

    [Header("Invincible Frame")]
    public float InvincibleTime = 0.5f;

    public bool IsDead = false;

    public void Heal(int amount)
    {
        Health += amount;
    }

    public bool TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            IsDead = true;
            Health = 0;
            return true;
        }
        return false;
    }

    public float GetHealthAmount()
    {
        return Health / MaxHealth;
    }

}
