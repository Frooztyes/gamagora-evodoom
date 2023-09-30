using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Ennemy", order = 1)]
public class Ennemy : ScriptableObject
{
    public enum AttackType
    {
        VOLLEY,
        LASER,
        SHOOT,
        MELEE
    }

    [Header("Attack attribute")]
    public Vector2 pivotPoint = Vector2.zero;
    public float AggroRadius = 0;
    public float ShootRadius = 0;

    public float Speed = 0;

    public bool CanLooseAggro = false;

    public AttackType AttackPattern;

    public GameObject VFX;
}
