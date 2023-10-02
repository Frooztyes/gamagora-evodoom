using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MyCharacterController : MonoBehaviour
{
    [Header("Ground collisions")]
    [SerializeField] private LayerMask ground;
    [SerializeField] private Transform groundChecker;

    [Header("Gun")]
    [SerializeField] private Gun gun;
    [SerializeField] private Transform gunPosition;
    
    [Header("Landing Events")]
    [SerializeField] private UnityEvent OnLandEvent;

    private Rigidbody2D rb;
    private bool defaultFacing;
    private bool isGrounded;
    private Quaternion qTo;
    private float rotationOffset = 0;
    private Transform gunGFX;
    private Animation gunAnimation;

    void Start()
    {
        gunGFX = Instantiate(gun.GFX, gunPosition.position, Quaternion.identity).transform;
        gunGFX.transform.parent = transform;
        gunGFX.localScale = Vector3.one * gun.Scale;
        gunAnimation = gunGFX.GetComponent<Animation>();
        qTo = gunGFX.transform.rotation;
        defaultFacing = transform.localScale.x > 0;
        rb = GetComponent<Rigidbody2D>();
        isGrounded = false;
    }

    public bool IsGrounded()
    {
        return isGrounded;
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

        if(gunGFX)
            gunGFX.transform.rotation = Quaternion.Slerp(gunGFX.transform.rotation, qTo, Time.fixedDeltaTime * gun.RotationReloadSpeed);
        if (!gunAnimation.isPlaying && gun.MagazineCapacity == 0)
        {
            gun.Reload();
        }
    }


    public void Move(float amount, float jumpForce, bool shooting)
    {
        if (amount < 0 && defaultFacing) FlipCharacter();
        else if (amount > 0 && !defaultFacing) FlipCharacter();

        transform.Translate(Vector3.right * amount);

        if (jumpForce > 0)
        {
            rb.AddForce(Vector2.up * jumpForce);
        }

        if (shooting)
        {
            //rb.AddForce((defaultFacing ? Vector3.left : Vector3.right) * gun.RecoilStrength);
            ShootProjectile();
        }
    }

    private Vector3 getMousePosition()
    {
        Vector3 screenPosDepth = Input.mousePosition;
        screenPosDepth.z = Vector3.Dot(
            Camera.main.transform.forward,
            transform.position - Camera.main.transform.position
        );
        return Camera.main.ScreenToWorldPoint(screenPosDepth);
    }

    private void ShootProjectile()
    {
        if (gun.MagazineCapacity <= 0) return;

        Projectile p = Instantiate(gun.projectile, gunPosition.position, Quaternion.identity).GetComponent<Projectile>();
        
        Vector2 dir = (getMousePosition() - gunGFX.GetChild(0).position).normalized;
        if (dir.x < 0 && defaultFacing || dir.x >= 0 && !defaultFacing)
            FlipCharacter();

        // angle pointing toward mouse position in the good direction
        qTo = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - rotationOffset);
        p.setDirection(dir);

        gun.MagazineCapacity--;
        if (gun.MagazineCapacity == 0)
        {
            gunAnimation.Play("Reload");
        }
    }

    void FlipCharacter() 
    {
        defaultFacing = !defaultFacing;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        rotationOffset = defaultFacing ? 0 : 180;
    }
}
