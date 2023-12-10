using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ONLY FOR TESTING PURPOSES, NOT IN THE ACTUAL GAME
/// </summary>
public class Cobaye : MonoBehaviour
{
    [SerializeField] private float vertSpeed = 100f;
    [SerializeField] private float horSpeed = 50f;

    Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            rb.AddForce(Vector2.up * vertSpeed + Vector2.right * horSpeed);
        }
    }
}
