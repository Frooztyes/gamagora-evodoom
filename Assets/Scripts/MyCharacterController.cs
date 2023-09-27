using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MyCharacterController : MonoBehaviour
{
    [Header("Forces")]
    [SerializeField] private float jumpForce = 50f;
    [SerializeField] private float shootForce= 400f;

    [Header("Ground collisions")]
    [SerializeField] private LayerMask ground;
    [SerializeField] private Transform groundChecker;

    [Header("Gun")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform projectilePosition;
    [SerializeField] private Transform gun;
    [SerializeField] private float magazineCapacity;
    [SerializeField] private float gunRotationSpeed = 2.0f;
    private Quaternion qTo;
    private float magazine;
    private float rotationOffset = 0;

    [SerializeField] private Animator gunAnimator;

    [Header("Landing Events")]
    [SerializeField] private UnityEvent OnLandEvent;

    private Rigidbody2D rb;

    private bool defaultFacing;
    private bool isGrounded;

    void Start()
    {
        qTo = gun.transform.rotation;
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

        gun.transform.rotation = Quaternion.Slerp(gun.transform.rotation, qTo, Time.fixedDeltaTime * gunRotationSpeed);
        if (/* animator playing &&*/ magazine == 0)
        {
            magazine = magazineCapacity;
        }
    }


    public void Move(float amount, bool flying, Vector2 dir)
    {
        if (amount < 0 && defaultFacing) FlipCharacter();
        else if (amount > 0 && !defaultFacing) FlipCharacter();
        transform.Translate(Vector3.right * amount);
        if (flying)
        {
            rb.AddForce(Vector2.up * jumpForce);
        }
        if (dir != Vector2.zero)
        {
            //rb.AddForce((defaultFacing ? Vector3.left : Vector3.right) * shootingStrengh);
            ShootProjectile(dir);
        }
    } 

    private void ShootProjectile(Vector2 dir)
    {
        Projectile p = Instantiate(projectile, projectilePosition.position, Quaternion.identity).GetComponent<Projectile>();


        if (dir.x < 0 && defaultFacing || dir.x >= 0 && !defaultFacing)
            FlipCharacter();

        // angle pointing toward mouse position in the good direction
        qTo = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - rotationOffset);
        p.setDirection(dir);

        magazine--;
        if (magazine == 0)
        {
            //gunAnimator.Play("Reload");
        }
        else
        {
            //gunAnimator.Play("Shoot");
        }
    }

    public bool getFacing()
    {
        return defaultFacing;
    }

    void FlipCharacter() 
    {
        defaultFacing = !defaultFacing;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        rotationOffset = defaultFacing ? 0 : 180;
    }
}
