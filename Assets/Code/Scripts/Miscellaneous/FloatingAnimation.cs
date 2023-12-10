using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// NOT USED, but can be in the future
/// </summary>
public class FloatingAnimation : MonoBehaviour
{
    Vector3 defaultPosition;

    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: à corriger, marche vite fait 
        // problème : ça tourne dans tout les sens
        defaultPosition = transform.parent.position;
        if (transform.position != defaultPosition)
        {
            Vector3 dir = (defaultPosition - transform.position);
            rb.AddForce(dir.normalized * dir.magnitude);
        }
        if(!locked)
        {
            rb.velocity *= Mathf.Clamp(Vector3.Distance(transform.position, defaultPosition) * 3, 0, 1);
            if (rb.velocity.magnitude < 0.01)
            {
                transform.position = Vector3.Slerp(transform.position, defaultPosition, Time.deltaTime * 2);
            }
        }
    }

    bool locked = false;

    private void Unlock()
    {
        locked = false;
    }

    public void Recoil(bool toRight)
    {
        locked = true;
        rb.AddForce((toRight ? 1 : -1) * 300 * transform.right);
        Invoke(nameof(Unlock), 0.5f);
    }
}
