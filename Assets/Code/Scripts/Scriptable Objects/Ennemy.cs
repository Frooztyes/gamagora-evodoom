using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Ennemy", order = 1)]
public class Ennemy : LivingCreature
{
    public enum AttackType
    {
        VOLLEY,
        LASER,
        SHOOT,
        MELEE,
        ROCKET
    }

    [Header("Attack attribute")]
    public Vector2 pivotPoint = Vector2.zero;
    public float AggroRadius = 0;
    public float ShootRadius = 0;

    public float GravityScale = 0;

    public bool CanLooseAggro = false;
    public bool CanMove = true;
    public bool CanRotate = true;

    public int ContactDamage = 10;

    public AttackType AttackPattern;

    public GameObject VFX;
}
