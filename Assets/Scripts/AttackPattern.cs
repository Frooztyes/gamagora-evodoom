using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackPattern : MonoBehaviour
{
    public bool HasFinished = true;

    public abstract void StartAttack();
    public abstract void StopAttack();
}
