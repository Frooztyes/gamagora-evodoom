using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ONLY FOR TESTING PURPOSES, NOT IN THE ACTUAL GAME
/// </summary>
public class PlayWeaponAttack : MonoBehaviour
{
    [SerializeField] AttackPattern attack;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        attack.StartAttack();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        attack.StopAttack();
    }

}
