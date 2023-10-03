using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
