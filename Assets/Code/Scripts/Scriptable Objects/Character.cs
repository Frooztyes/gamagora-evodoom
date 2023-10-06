using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Character", order = 1)]
public class Character : LivingCreature
{
    [Header("Levitation")]
    public float levitationCapacity = 40f;
    public float currentLevitationCapacity = 40f;
    public float levitationRecoveryPerTick = 1f;
    public float levitationUsedPerTick = 1f;

    [Range(0f, 1f)]
    public float levitationReductionFlying = 1 / 3;

    public float GetJumpForce(bool flying)
    {
        if (!flying || currentLevitationCapacity <= 0) return 0;
        return JumpForce;
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
}
