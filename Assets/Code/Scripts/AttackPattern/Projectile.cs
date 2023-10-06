using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float velocity = 10f;
    [SerializeField] private float accelerationPerTick = 0.5f;

    private int damage = 0;

    private bool initialized = false;
    private int parentLayer = -1;
    
    void Update()
    {
        if (!initialized) return;
        velocity += accelerationPerTick;
        transform.Translate(Time.deltaTime * velocity * Vector2.right);
    }

    public void SetStatistics(Vector2 dir, int damage, int parentLayer)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        this.damage = damage;
        this.parentLayer = parentLayer;
        initialized = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int layer = collision.gameObject.layer;
        if (layer == LayerMask.NameToLayer("Player") && parentLayer != collision.gameObject.layer)
        {
            collision.gameObject.GetComponent<MyCharacterController>().TakeDamage(damage, collision.transform.position.x < transform.position.x);
            Destroy(gameObject);
        }
        if (layer == LayerMask.NameToLayer("Ennemy") && parentLayer != collision.gameObject.layer)
        {
            Debug.Log(collision.gameObject.name);
            collision.gameObject.GetComponentInParent<EnnemyAI>().TakeDamage(damage, false);
            Destroy(gameObject);
        }
        if (layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
