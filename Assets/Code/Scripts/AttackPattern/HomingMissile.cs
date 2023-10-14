using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HomingMissile : MonoBehaviour
{

    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotateSpeed = 200f;
    [SerializeField] private float acceleration = 0.01f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float explosionRadius = 10f;
    private Transform target;

    public GameObject explosionEffect;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
    }


    void FixedUpdate()
    {
        Vector2 direction = (Vector2) target.position - rb.position;

        direction.Normalize();

        float rotateAmount = Vector3.Cross(direction, transform.up).z;

        rb.angularVelocity = -rotateAmount * rotateSpeed;

        rb.velocity = transform.up * speed;
        speed += acceleration;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        string layer = LayerMask.LayerToName(collision.gameObject.layer);
        if (layer == "Player")
        {
            collision.gameObject.GetComponent<MyCharacterController>().TakeDamage(collision.transform.position.x < transform.position.x);
            Instantiate(explosionEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
        if (layer == "Ground")
        {
            var hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Ennemy"));
            foreach(Collider2D i in hitColliders)
            {
                layer = LayerMask.LayerToName(i.gameObject.layer);
                if(i.gameObject.GetComponent<MyCharacterController>())
                    i.gameObject.GetComponent<MyCharacterController>().TakeDamage(collision.transform.position.x < transform.position.x);
                if (i.gameObject.GetComponent<EnnemyAI>())
                    i.gameObject.GetComponent<EnnemyAI>().TakeDamage(damage, collision.transform.position.x < transform.position.x);
            }
            Instantiate(explosionEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

}
