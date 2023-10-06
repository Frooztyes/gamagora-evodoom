using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Character", order = 1)]
public class Character : ScriptableObject
{
    [Header("Health")]
    public float MaxHealth = 100f;
    public float Health = 100f;

    [Header("Movements")]
    public float RunSpeed = 40f;
    public float JumpForce = 80f;

    [Header("Levitation")]
    public float levitationCapacity = 40f;
    public float currentLevitationCapacity = 40f;
    public float levitationRecoveryPerTick = 1f;
    public float levitationUsedPerTick = 1f;

    [Range(0f, 1f)]
    public float levitationReductionFlying = 1 / 3;

    public float InvincibleTime = 0.5f;

    private bool isDead = true;

    public float GetJumpForce(bool flying)
    {
        if (!flying || currentLevitationCapacity <= 0) return 0;
        return JumpForce;
    }

    public void Heal(int amount)
    {
        Health += amount;
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0) 
        {
            isDead = true;
            Health = 0;
        }
    }

    public float GetLevitationFillAmount()
    {
        return currentLevitationCapacity / levitationCapacity;
    }

    public void UpdateLevitation(bool flying, bool isGrounded)
    {
        // reduce recovery while not on ground
        float internalRecoveryByTick = levitationRecoveryPerTick;
        if (!isGrounded)
        {
            internalRecoveryByTick *= levitationReductionFlying;
        }

        // reducing levitation bar while flying and increasing while not
        currentLevitationCapacity += flying ? -levitationUsedPerTick : internalRecoveryByTick;
        // clamping capacity to not be less than 0 and being more than the capacity
        currentLevitationCapacity = Mathf.Clamp(currentLevitationCapacity, 0, levitationCapacity);
    }

    public float GetHealthAmount()
    {
        return Health / MaxHealth;
    }
}
