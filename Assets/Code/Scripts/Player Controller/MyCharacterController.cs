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

    [Header("Sounds")]
    [SerializeField] private List<AudioClip> walkingSounds;
    [SerializeField] private AudioSource otherSoundsEffect;
    [SerializeField] private AudioClip coinSound;

    [SerializeField] private Animator animator;
    [SerializeField] private Transform flashLight;

    private Character editableChar;
    private Gun editableGun;
    private AudioSource characterAudioSource;

    private Rigidbody2D rb;
    private bool defaultFacing;
    private bool isGrounded;
    private Quaternion qTo;
    private float rotationOffset = 0;
    private Transform gunGFX;
    private Animation gunAnimation;

    private Image levitationIndicator;
    private AudioSource[] gunSounds;
    private SpriteRenderer sprite;

    // variable de framing invincible
    private bool isInvincible;
    private bool isStunned = false;

    private HealthBars healthBars;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        editableChar = Instantiate(character);
        editableGun = Instantiate(gun);
        gunGFX = Instantiate(editableGun.GFX, gunPosition.position, Quaternion.identity).transform;
        gunGFX.transform.parent = gunPosition;
        gunGFX.localScale = Vector3.one * editableGun.Scale;
        gunAnimation = gunGFX.GetComponent<Animation>();
        gunSounds = gunGFX.GetComponents<AudioSource>();
        qTo = gunGFX.transform.rotation;
        defaultFacing = transform.localScale.x > 0;
        rb = GetComponent<Rigidbody2D>();
        isGrounded = false;
        levitationIndicator = GameObject.FindGameObjectWithTag("LevitationBar").GetComponent<Image>();
        healthBars = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<HealthBars>();
        healthBars.SetHealth(editableChar.Health);
        healthBars.AddSheild();


        characterAudioSource = GetComponent<AudioSource>();
    }

    private void OnDrawGizmosSelected()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
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
            gunGFX.transform.rotation = Quaternion.Slerp(gunGFX.transform.rotation, qTo, Time.fixedDeltaTime * editableGun.RotationSpeed);
        if (!gunAnimation.isPlaying && editableGun.MagazineCapacity == 0)
        {
            editableGun.Reload();
        }

        if(isGrounded && isStunned)
        {
            isStunned = false;
        }

        Vector2 positionOnScreen = Camera.main.WorldToViewportPoint(transform.position);

        //Get the Screen position of the mouse
        Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);

        //Get the angle between the points
        float angle = AngleBetweenTwoPoints(positionOnScreen, mouseOnScreen);

        flashLight.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle + 90f));
    }
    void PlayRandomWalkingSound()
    {
        int randomIndex = Random.Range(0, walkingSounds.Count);
        characterAudioSource.clip = walkingSounds[randomIndex];
        characterAudioSource.Play();
    }

    public void Move(float amount, bool flying, bool shooting)
    {
        if (!characterAudioSource.isPlaying) characterAudioSource.clip = null;
        isFlying = flying;
        if (isStunned) return;
        animator.SetFloat("Speed", Mathf.Abs(amount));
        if (flying)
        {
            animator.SetBool("IsJumping", flying);
        }

        amount *= editableChar.MoveSpeed;

        if (amount < 0 && defaultFacing) FlipCharacter();
        else if (amount > 0 && !defaultFacing) FlipCharacter();

        transform.Translate(Vector3.right * amount);
        if(Mathf.Abs(amount) > 0.01f && !characterAudioSource.isPlaying && !flying && isGrounded)
            PlayRandomWalkingSound();

        rb.AddForce(Vector2.up * editableChar.GetJumpForce(flying));
        editableChar.UpdateLevitation(flying, isGrounded);
        levitationIndicator.fillAmount = editableChar.GetLevitationFillAmount();

        if (shooting)
        {
            rb.AddForce((defaultFacing ? Vector3.left : Vector3.right) * editableGun.RecoilStrength);
            ShootProjectile();
        }

        if((!isGrounded || isFlying) && !shooting)
        {
            //gunGFX.position = gunPosition.position;
        }
    }

    public void TakeDamage(bool fromRight, int damage = 1)
    {
        if (isInvincible) return;
        editableChar.TakeDamage(damage);
        healthBars.RemoveOne();
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
        Vector2 dir = (getMousePosition() - gunGFX.GetChild(0).position).normalized;
        if (dir.x < 0 && defaultFacing || dir.x >= 0 && !defaultFacing)
            FlipCharacter();

        // angle pointing toward mouse position in the good direction
        qTo = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - rotationOffset);
        p.SetStatistics(dir, editableGun.Damage, gameObject.layer);

        editableGun.MagazineCapacity--;
        if (editableGun.MagazineCapacity == 0)
        {
            gunAnimation.Play();
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
        if (layer == LayerMask.NameToLayer("Collectible"))
        {
            if (collision.gameObject.tag == "Coin")
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
}
