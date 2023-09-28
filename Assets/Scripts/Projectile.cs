using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float velocity = 10f;
    [SerializeField] private float accelerationPerTick = 0.5f;

    private bool initialized = false;

    void Update()
    {
        if (!initialized) return;
        velocity += accelerationPerTick;
        transform.Translate(Time.deltaTime * velocity * Vector2.right);
    }

    public void setDirection(Vector2 dir)
    {
        //if (dir.x < 0) transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        initialized = true;
    }
}
