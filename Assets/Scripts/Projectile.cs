using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float velocity = 10f;
    [SerializeField] private float accelerationPerTick = 0.5f;

    private Vector2 dir;
    private bool initialized = false;

    void Update()
    {
        if (!initialized) return;
        velocity += accelerationPerTick;
        transform.Translate(dir * velocity * Time.deltaTime);
    }

    public void setDirection(Vector2 dir)
    {
        this.dir = dir;
        initialized = true;
    }
}
