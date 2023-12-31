using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Gun", order = 1)]
public class Gun : ScriptableObject
{
    public int MaxMagazineCapacity = 3;
    public int MagazineCapacity = 3;

    public float RotationReloadSpeed = 2f;

    public float RecoilStrength = 0f;

    public int Damage;

    public GameObject projectile;
    public GameObject GFX;
    public float Scale;

    public void Reload()
    {
        MagazineCapacity = MaxMagazineCapacity;
    }
}
