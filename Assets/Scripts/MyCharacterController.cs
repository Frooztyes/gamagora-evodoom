using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class MyCharacterController : MonoBehaviour
{
    [SerializeField] private Character character;

    [Header("Ground collisions")]
    [SerializeField] private LayerMask ground;
    [SerializeField] private Transform groundChecker;

    [Header("Gun")]
    [SerializeField] private Gun gun;
    [SerializeField] private Transform gunPosition;
    
    [Header("Landing Events")]
    [SerializeField] private UnityEvent OnLandEvent;

    [SerializeField] private Animator animator;

    private Character editableChar;
    private Gun editableGun;

    private Rigidbody2D rb;
    private bool defaultFacing;
    private bool isGrounded;
    private Quaternion qTo;
    private float rotationOffset = 0;
    private Transform gunGFX;
    private Animation gunAnimation;

    private Image levitationIndicator;
    private Image healthIndicator;
    private SpriteRenderer sprite;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        editableChar = Instantiate(character);
        editableGun = Instantiate(gun);
        gunGFX = Instantiate(editableGun.GFX, gunPosition.position, Quaternion.identity).transform;
        gunGFX.transform.parent = transform;
        gunGFX.localScale = Vector3.one * editableGun.Scale;
        gunAnimation = gunGFX.GetComponent<Animation>();
        qTo = gunGFX.transform.rotation;
        defaultFacing = transform.localScale.x > 0;
        rb = GetComponent<Rigidbody2D>();
        isGrounded = false;
        levitationIndicator = GameObject.FindGameObjectWithTag("LevitationBar").GetComponent<Image>();
        healthIndicator = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<Image>();
        healthIndicator.fillAmount = editableChar.GetHealthAmount();
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    private bool isFlying;

    private void FixedUpdate()
    {
        isGrounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundChecker.position, 0.5f, ground);

        foreach (Collider2D col in  colliders)
        {
            if(col.gameObject != gameObject && !isFlying)
            {
                isGrounded = true;
                animator.SetBool("IsJumping", false);
            }
        }

        if(gunGFX)
            gunGFX.transform.rotation = Quaternion.Slerp(gunGFX.transform.rotation, qTo, Time.fixedDeltaTime * editableGun.RotationReloadSpeed);
        if (!gunAnimation.isPlaying && editableGun.MagazineCapacity == 0)
        {
            editableGun.Reload();
        }

        if(isGrounded && isStunned)
        {
            isStunned = false;
        }
    }


    public void Move(float amount, bool flying, bool shooting)
    {
        isFlying = flying;
        if (isStunned) return;
        animator.SetFloat("Speed", Mathf.Abs(amount));
        if (flying)
        {
            animator.SetBool("IsJumping", flying);
        }

        amount *= editableChar.RunSpeed;

        if (amount < 0 && defaultFacing) FlipCharacter();
        else if (amount > 0 && !defaultFacing) FlipCharacter();

        transform.Translate(Vector3.right * amount);

        rb.AddForce(Vector2.up * editableChar.GetJumpForce(flying));
        editableChar.UpdateLevitation(flying, isGrounded);
        levitationIndicator.fillAmount = editableChar.GetLevitationFillAmount();

        if (shooting)
        {
            rb.AddForce((defaultFacing ? Vector3.left : Vector3.right) * editableGun.RecoilStrength);
            ShootProjectile();
        }
    }

    // variable de framing invincible
    private bool isInvincible;
    private bool isStunned = false;

    public void TakeDamage(int damage, bool fromRight)
    {
        if (isInvincible) return;
        editableChar.TakeDamage(damage);
        healthIndicator.fillAmount = editableChar.GetHealthAmount();
        isInvincible = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = 0;
        if (isFlying || !isGrounded)
        {
            isStunned = true;
        } 
        else
        {
            rb.AddForce(Vector2.up * 500);
        }
        rb.AddForce((fromRight ? Vector3.left : Vector3.right) * 200);

        InvokeRepeating(nameof(BlinkRed), 0, 0.2f);
        Invoke(nameof(EndInvincibleFrame), editableChar.InvincibleTime);

    }

    private bool IsRed = false;
    public void BlinkRed()
    {
        IsRed = !IsRed;
        sprite.color = IsRed ? Color.red : Color.white;
    }

    private void EndInvincibleFrame()
    {
        CancelInvoke(nameof(BlinkRed));
        isInvincible = false;
        if(IsRed) BlinkRed();
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
        if (editableGun.MagazineCapacity <= 0) return;

        Projectile p = Instantiate(editableGun.projectile, gunPosition.position, Quaternion.identity).GetComponent<Projectile>();
        
        Vector2 dir = (getMousePosition() - gunGFX.GetChild(0).position).normalized;
        if (dir.x < 0 && defaultFacing || dir.x >= 0 && !defaultFacing)
            FlipCharacter();

        // angle pointing toward mouse position in the good direction
        qTo = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - rotationOffset);
        p.SetStatistics(dir, editableGun.Damage);

        editableGun.MagazineCapacity--;
        if (editableGun.MagazineCapacity == 0)
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
