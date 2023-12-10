using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ScriptableObject representing a Enemmy.
/// </summary>
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
    /// <summary>
    /// Circle around enemmy to start moving to player
    /// </summary>
    public float AggroRadius = 0;
    /// <summary>
    /// Circle around enemmy to start attacking player
    /// </summary>
    public float ShootRadius = 0;
    public int ContactDamage = 10;

    /// <summary>
    /// If coin loot was implemented
    /// </summary>
    public int MinCoinsOnDeath = 2;
    public int MaxCoinsOnDeath = 10;

    /// <summary>
    /// 0 if it's a flying enemmy, else can be anything > 0
    /// </summary>
    public float GravityScale = 0;

    /// <summary>
    /// Enemmy stop moving to player if he goes too far
    /// </summary>
    public bool CanLooseAggro = false;

    /// <summary>
    /// Allow enemmy to move (useful for turrets)
    /// </summary>
    public bool CanMove = true;

    /// <summary>
    /// Allow enemmy to rotate in an other direction (for turrets)
    /// </summary>
    public bool CanRotate = true;

    /// <summary>
    /// Type of attack
    /// </summary>
    public AttackType AttackPattern;

    /// <summary>
    /// GameObject representing the Sprite of the enemmy
    /// </summary>
    public GameObject VFX;
}
