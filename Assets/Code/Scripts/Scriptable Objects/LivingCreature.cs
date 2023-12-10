using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject representing living creature :
/// - Enemmy 
/// - Player
/// </summary>
public abstract class LivingCreature : ScriptableObject
{
    [Header("Health")]
    public int MaxHealth = 5;
    public int Health = 5;

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

    /// <summary>
    /// Remove health when taking damage
    /// </summary>
    /// <param name="damage"></param>
    /// <returns>True if dead</returns>
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

    /// <summary>
    /// Return percentage of health over max health
    /// </summary>
    /// <returns></returns>
    public float GetHealthAmount()
    {
        return Health / MaxHealth;
    }

}
