using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackPattern : MonoBehaviour
{
    protected bool hasFinished = true;
    public abstract bool IsOver();
    public abstract void StartAttack();
    public abstract void StopAttack();
}
