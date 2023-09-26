using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MyCharacterController : MonoBehaviour
{
    [SerializeField] private float jumpStrength = 50f;
    [SerializeField] private float shootingStrengh = 400f;
    [SerializeField] private LayerMask ground;
    [SerializeField] private Transform groundChecker;
    [SerializeField] private UnityEvent OnLandEvent;

    private Rigidbody2D rb;

    private bool defaultFacing;
    private bool isGrounded;

    void Start()
    {
        defaultFacing = transform.localScale.x > 0;
        rb = GetComponent<Rigidbody2D>();
        isGrounded = false;
    }

    private void FixedUpdate()
    {
        isGrounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundChecker.position, 0.5f, ground);

        foreach (Collider2D col in  colliders)
        {
            if(col.gameObject != gameObject)
            {
                isGrounded = true;
                OnLandEvent.Invoke();
            }
        }
    }

    public bool Move(float amount, bool flying, bool shooting)
    {
        if (amount < 0 && defaultFacing) FlipCharacter();
        else if (amount > 0 && !defaultFacing) FlipCharacter();
        transform.Translate(Vector3.right * amount);
        if (flying)
        {
            rb.AddForce(Vector2.up * jumpStrength);
        }
        if (shooting && !isGrounded)
        {
            //rb.AddForce((defaultFacing ? Vector3.left : Vector3.right) * shootingStrengh);
        }
        return defaultFacing;
    } 

    public bool getFacing()
    {
        return defaultFacing;
    }

    void FlipCharacter() 
    {
        defaultFacing = !defaultFacing;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }
}
