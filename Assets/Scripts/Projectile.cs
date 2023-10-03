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
    
    void Update()
    {
        if (!initialized) return;
        velocity += accelerationPerTick;
        transform.Translate(Time.deltaTime * velocity * Vector2.right);
    }

    public void setStatistics(Vector2 dir, int damage)
    {
        //if (dir.x < 0) transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        this.damage = damage;
        initialized = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int layer = collision.gameObject.layer;
        if (layer == LayerMask.NameToLayer("Player"))
        {
            
            collision.gameObject.GetComponent<MyCharacterController>().TakeDamage(damage, collision.transform.position.x < transform.position.x);
        }
        if (layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
