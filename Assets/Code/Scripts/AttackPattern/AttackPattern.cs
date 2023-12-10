using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class representing any attack patterns
/// </summary>
public abstract class AttackPattern : MonoBehaviour
{
    protected bool hasFinished = true;
    public abstract bool IsOver();
    public abstract void StartAttack();
    public abstract void StopAttack();
}
