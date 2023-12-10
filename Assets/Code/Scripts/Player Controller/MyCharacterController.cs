using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

    [Header("Sounds")]
    [SerializeField] private List<AudioClip> walkingSounds;
    [SerializeField] private AudioSource otherSoundsEffect;
    public AudioSource deathSound;
    [SerializeField] private AudioClip coinSound;

    [SerializeField] private Animator animator;

    [SerializeField] private Sprite reloadIcon;
    [SerializeField] private Color HighAmmoColor;
    [SerializeField] private Color MediumAmmoColor;
    [SerializeField] private Color LowAmmoColor;
    [SerializeField] private Color EmptyAmmoColor;

    private Sprite gunIcon;

    public Character EditableChar { get; private set; }
    private Gun editableGun;
    private AudioSource characterAudioSource;

    private Rigidbody2D rb;


    // Slope Handling
    private CapsuleCollider2D myCollider;
    private Vector2 colliderSize;
    [SerializeField] private float slopeCheckDistance;
    private float slopeDownAngle;
    private Vector2 slopeNormalPerp;
    private bool isOnSlope;
    private float slopeDownAngleOld;
    private float slopeSizeAngle;
    [SerializeField] private PhysicsMaterial2D noFriction;
    [SerializeField] private PhysicsMaterial2D fullFriction;

    private bool defaultFacing;
    private bool isGrounded;
    private Quaternion qTo;
    private float rotationOffset = 0;
    private Transform gunGFX;
    private Animation gunAnimation;

    private Image levitationIndicator;
    private Image dodgeIndicator;
    private Image ammoBar;
    private Transform cursor;
    private AudioSource[] gunSounds;
    private SpriteRenderer sprite;
    private TextMeshProUGUI maxAmmo;
    private TextMeshProUGUI currentAmmo;
    private Image reloadUI;

    // variable de framing invincible
    private bool isInvincible;
    private bool isStunned = false;

    private HealthBars healthBars;

    private float cooldownDashInternal;

    void Start()
    {
        //if(Input.GetJoystickNames().Length > 0)
        //    Cursor.visible = false;

        myCollider = GetComponent<CapsuleCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        characterAudioSource = GetComponent<AudioSource>();

        // Slopes handling
        colliderSize = myCollider.size;

        EditableChar = Instantiate(character);
        editableGun = Instantiate(gun);

        gunGFX = Instantiate(editableGun.GFX, gunPosition.position, Quaternion.identity).transform;
        gunGFX.transform.parent = gunPosition;
        gunGFX.localScale = Vector3.one * editableGun.Scale;
        gunAnimation = gunGFX.GetComponent<Animation>();
        gunSounds = gunGFX.GetComponents<AudioSource>();

        qTo = gunGFX.transform.rotation;

        defaultFacing = transform.localScale.x > 0;
        isGrounded = false;

        // getting HUD components
        levitationIndicator = GameObject.FindGameObjectWithTag("LevitationBar").GetComponent<Image>();
        healthBars = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<HealthBars>();
        dodgeIndicator = GameObject.FindGameObjectWithTag("DodgeBar").GetComponent<Image>();
        ammoBar = GameObject.FindGameObjectWithTag("AmmoBar").GetComponent<Image>();
        maxAmmo = ammoBar.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        currentAmmo = ammoBar.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        reloadUI = GameObject.FindGameObjectWithTag("ReloadIcon").GetComponent<Image>();
        cursor = GameObject.FindGameObjectWithTag("Cursor").GetComponent<Transform>();

        gunIcon = reloadUI.sprite;

        healthBars.SetHealth(EditableChar.Health);
        for (int i = 0; i < EditableChar.DefaultSheild; i++)
        {
            healthBars.AddSheild();
        }

        currentAmmo.text = editableGun.MagazineCapacity.ToString();
        maxAmmo.text = editableGun.MaxMagazineCapacity.ToString();

        cooldownDashInternal = EditableChar.cooldownDash;
        EditableChar.currentLevitationCapacity = EditableChar.levitationCapacity;
    }

    private void SlopeCheck()
    {
        Vector2 checkPosition = transform.position - new Vector3(0, colliderSize.y / 2);
        SlopeCheckHorizontal(checkPosition);
        SlopeCheckVertical(checkPosition);

    }

    private void SlopeCheckHorizontal(Vector2 checkPosition)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPosition, transform.right, slopeCheckDistance, ground);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPosition, -transform.right, slopeCheckDistance, ground);

        if(slopeHitFront)
        {
            isOnSlope = true;
            slopeSizeAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
        } 
        else if(slopeHitBack)
        {
            isOnSlope = true;
            slopeSizeAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            slopeSizeAngle = 0.0f;
            isOnSlope = false;
        }
    }

    private void SlopeCheckVertical(Vector2 checkPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPosition, Vector2.down, slopeCheckDistance, ground);
        if(hit)
        {
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if(slopeDownAngle != slopeDownAngleOld)
            {
                isOnSlope = true;
            }

            slopeDownAngleOld = slopeDownAngle;

            Debug.DrawRay(hit.point, slopeNormalPerp, Color.red);

            Debug.DrawRay(hit.point, hit.normal, Color.green);
        }

        if(isOnSlope && locAmount == 0.0f) 
        {
            rb.sharedMaterial = fullFriction;
        } 
        else
        {
            rb.sharedMaterial = noFriction;
        } 
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }
#endif

    public bool IsGrounded()
    {
        return isGrounded;
    }

    private bool isFlying;

    private void FixedUpdate()
    {
        if (EditableChar.IsDead)
        {
            return;
        }
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
            gunGFX.transform.rotation = Quaternion.Slerp(gunGFX.transform.rotation, qTo, Time.fixedDeltaTime * editableGun.RotationSpeed);
        if (!gunAnimation.isPlaying && isReloading)
        {
            reloadUI.transform.rotation = Quaternion.Euler(Vector3.zero);
            reloadUI.sprite = gunIcon;
            editableGun.Reload();
            ammoBar.color = HighAmmoColor;
            isReloading = false;
        }
        if(editableGun.MagazineCapacity == 0)
        {
            reloadUI.transform.Rotate(Vector3.forward * 10);
        }

        if(isGrounded && isStunned)
        {
            isStunned = false;
        }

        //SlopeCheck();
    }

    private bool isReloading = false;

    void PlayRandomWalkingSound()
    {
        int randomIndex = Random.Range(0, walkingSounds.Count);
        characterAudioSource.clip = walkingSounds[randomIndex];
        characterAudioSource.Play();
    }

    private bool IsAnimationFinished(string animationName)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animationName)
               && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1
               && !animator.IsInTransition(0);
    }

    private void UpdateBars()
    {
        EditableChar.UpdateLevitation(isFlying, isGrounded);
        levitationIndicator.fillAmount = EditableChar.GetLevitationFillAmount();
        dodgeIndicator.fillAmount = Mathf.Clamp(cooldownDashInternal, 0, EditableChar.cooldownDash) / EditableChar.cooldownDash;
        currentAmmo.text = editableGun.MagazineCapacity.ToString();
        maxAmmo.text = editableGun.MaxMagazineCapacity.ToString();
    }

    public bool hasDodge = false;
    private float locAmount;
    public void Move(float amount, bool flying, bool shooting, bool dodge, bool reloading, Vector2 cursorAiming)
    {
        if (EditableChar.IsDead)
        {
            animator.SetBool("IsDead", true);
            return;
        }

        locAmount = amount;
        isFlying = flying;

        // updating dash cooldown
        if (cooldownDashInternal <= EditableChar.cooldownDash)
            cooldownDashInternal += Time.fixedDeltaTime;

        if (IsAnimationFinished("Cyborg_dash"))
        {
            animator.SetBool("IsDodging", false);
            hasDodge = false;
            rb.velocity = Vector2.up * rb.velocity;
        }

        UpdateBars();

        SetCursorPosition(cursorAiming);

        // can't do any action if stunned or dodging
        if (isStunned || hasDodge) return;



        animator.SetFloat("Speed", Mathf.Abs(amount));

        if (flying)
            animator.SetBool("IsJumping", flying);

        if (shooting && !isReloading)
        {
            rb.AddForce((defaultFacing ? Vector3.left : Vector3.right) * editableGun.RecoilStrength);
            ShootProjectile();
        }


        // sound event
        if (!characterAudioSource.isPlaying) characterAudioSource.clip = null;
        if (Mathf.Abs(amount * EditableChar.MoveSpeed) > 0.01f && !characterAudioSource.isPlaying && !flying && isGrounded)
            PlayRandomWalkingSound();

        // dodge event
        if (dodge && cooldownDashInternal >= EditableChar.cooldownDash)
        {
            animator.SetBool("IsDodging", true);
            hasDodge = true;
            rb.AddForce((defaultFacing ? 1 : -1) * EditableChar.DashSpeed * Vector2.right);
            cooldownDashInternal = 0;
            return;
        }

        // fliping direction
        if (amount < 0 && defaultFacing) FlipCharacter();
        else if (amount > 0 && !defaultFacing) FlipCharacter();

        // movement force
        rb.velocity = new Vector2(amount * EditableChar.MoveSpeed, rb.velocity.y);
        // jump force
        rb.AddForce(Vector2.up * EditableChar.GetJumpForce(flying));
        //if(isGrounded && !isOnSlope)
        //{
        //    rb.velocity = new Vector2(amount * EditableChar.MoveSpeed, rb.velocity.y);
        //} 
        //else if(isGrounded && isOnSlope)
        //{
        //    rb.velocity = new Vector2(
        //        -amount * slopeNormalPerp.x * EditableChar.MoveSpeed,
        //        -amount * slopeNormalPerp.y * EditableChar.MoveSpeed
        //        );
        //} 
        //else if(!isGrounded)
        //{
        //    rb.velocity = new Vector2(amount * EditableChar.MoveSpeed, rb.velocity.y);
        //}

        if(reloading && !isReloading)
        {
            isReloading = true;
            reloadUI.sprite = reloadIcon;
            gunAnimation.Play();
        }

        if ((!isGrounded || isFlying) && !shooting)
        {
            //gunGFX.position = gunPosition.position;
        }

    }
    private Vector2 cursorDir;
    private readonly float desiredRot;
    private void SetCursorPosition(Vector2 position)
    {
        var desiredRotQ = Quaternion.Euler(Vector3.forward * Vector2.SignedAngle(Vector2.right, cursorDir));
        var desiredRotQFlashLight = Quaternion.Euler(Vector3.forward * (Vector2.SignedAngle(Vector2.right, cursorDir) - 90));
        //flashLight.rotation = Quaternion.Lerp(flashLight.rotation, desiredRotQFlashLight, Time.deltaTime * 20);
        cursor.transform.rotation = Quaternion.Lerp(cursor.transform.rotation, desiredRotQ, Time.deltaTime * 20);
        cursor.transform.GetChild(0).localRotation = Quaternion.Euler(cursor.transform.rotation.eulerAngles.z * -Vector3.forward);
        cursor.transform.position = gunGFX.GetChild(0).position;
        if (position.magnitude < 0.9 && Input.GetJoystickNames().Length != 0)
        {
            cursor.gameObject.SetActive(true);
            return;
        }
        if (Input.GetJoystickNames().Length == 0)
        {
            position = (GetMousePosition() - gunGFX.GetChild(0).position).normalized;
            cursor.gameObject.SetActive(false);
        } 
        else
        {
            position.y *= -1;
        }


        //cursor.transform.GetChild(0).localRotation = Quaternion.Euler(-Vector3.forward * Vector2.SignedAngle(Vector2.right, position));
        cursor.gameObject.SetActive(true);
        cursorDir = position;
    }

    public void TakeDamage(bool fromRight, int damage = 1)
    {
        if (isInvincible || EditableChar.IsDead) return;
        if(!EditableChar.TakeDamage(damage))
        {
            healthBars.RemoveOne();
        } 
        else
        {
            animator.SetBool("IsDead", true);
            return;
        }
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
        Invoke(nameof(EndInvincibleFrame), EditableChar.InvincibleTime);

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

    private Vector3 GetMousePosition()
    {
        Vector3 screenPosDepth = Input.mousePosition;
        screenPosDepth.z = Vector3.Dot(
            Camera.main.transform.forward,
            transform.position - Camera.main.transform.position
        );
        return Camera.main.ScreenToWorldPoint(screenPosDepth);
    }
    float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }

    private void ShootProjectile()
    {
        if (editableGun.MagazineCapacity <= 0) return;

        Projectile p = Instantiate(editableGun.projectile, gunGFX.position, Quaternion.identity).GetComponent<Projectile>();
        foreach (AudioSource gunSound in gunSounds)
        {
            gunSound.Play();
        }

        Vector2 dir = cursorDir;
        if (dir.x < 0 && defaultFacing || dir.x >= 0 && !defaultFacing)
            FlipCharacter();

        // angle pointing toward mouse position in the good direction
        qTo = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - rotationOffset);
        p.SetValues(dir, editableGun.Damage, gameObject.layer);

        editableGun.MagazineCapacity--;
        if (editableGun.MagazineCapacity == 0 && !isReloading)
        {
            reloadUI.sprite = reloadIcon;
            gunAnimation.Play();
            isReloading = true;
        }

        float percentMagazine = (float)editableGun.MagazineCapacity / editableGun.MaxMagazineCapacity;
        if(percentMagazine == 0)
        {
            ammoBar.color = EmptyAmmoColor;
        } 
        else if (percentMagazine <= 1.0f / 3)
        {
            ammoBar.color = LowAmmoColor;
        }
        else if (percentMagazine <= 2.0f / 3)
        {
            ammoBar.color = MediumAmmoColor;
        }
        else
        {
            ammoBar.color = HighAmmoColor;
        }

        gunGFX.GetComponent<FloatingAnimation>().Recoil(dir.x < 0);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        int layer = collision.gameObject.layer;
        if (layer == LayerMask.NameToLayer("Ennemy"))
        {
            TakeDamage(collision.transform.position.x > transform.position.x);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int layer = collision.gameObject.layer;
        if (layer == LayerMask.NameToLayer("Collectible") && !EditableChar.IsDead)
        {
            if (collision.gameObject.CompareTag("Coin"))
            {
                otherSoundsEffect.clip = coinSound;
                otherSoundsEffect.Play();
                Destroy(collision.gameObject);
            }
        }
    }


    void FlipCharacter() 
    {
        defaultFacing = !defaultFacing;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        rotationOffset = defaultFacing ? 0 : 180;
    }

    public void DeactivateCursor()
    {
        cursor.gameObject.SetActive(false);
    }
}
