using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform projectilePosition;
    [SerializeField] private Transform gun;

    [SerializeField] private MyCharacterController controller;

    [SerializeField] private Image radialLevitationIndicator;

    [SerializeField] private Animator animator;
    [SerializeField] private float runSpeed = 40f;

    [SerializeField] private float maxLevitationCapacity = 40f;

    private float levitationCapacity;
    private float levitationRecoveryPerTick = 1;
    private readonly float levitationUsedPerTick = 1;

    private float defaultLevitationRecoveryPerTick;

    private float horizontalMove = 0f;
    private bool flying;
    private bool shooting;

    [SerializeField] private float gunRotationSpeed = 2.0f;
    private Quaternion qTo;

    // Start is called before the first frame update
    void Start()
    {
        qTo = gun.transform.rotation;
        levitationCapacity = maxLevitationCapacity;
        defaultLevitationRecoveryPerTick = levitationRecoveryPerTick;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if(Input.GetButton("Jump"))
        {
            flying = true;
            animator.SetBool("IsJumping", true);
        }


        if (Input.GetMouseButtonDown(0))
        {
            shooting = true;
            Shoot();
        }

        gun.transform.rotation = Quaternion.Slerp(gun.transform.rotation, qTo, Time.deltaTime * gunRotationSpeed);
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

    private float rotationOffset = 0;
    private bool facing = true;
    private Vector2 dir;

    private void Shoot()
    {
        Projectile p = Instantiate(projectile, projectilePosition.position, Quaternion.identity).GetComponent<Projectile>();

        dir = (getMousePosition() - transform.position).normalized;

        if (dir.x < 0 && gun.localScale.x > 0)
        {
            gun.localScale = new Vector3(-gun.localScale.x, gun.localScale.y, gun.localScale.z);
            rotationOffset = 180;
        }
        else if (dir.x >= 0 && gun.localScale.x < 0)
        {
            gun.localScale = new Vector3(-gun.localScale.x, gun.localScale.y, gun.localScale.z);
            rotationOffset = 0;
        }

        qTo = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - rotationOffset);

        p.setDirection(dir);
    }

    public void OnLanding()
    {
        levitationRecoveryPerTick = defaultLevitationRecoveryPerTick;
        animator.SetBool("IsJumping", false);
    }

    private void FixedUpdate()
    {
        // reduce recovery while flying
        if(flying)
        {
            levitationRecoveryPerTick = defaultLevitationRecoveryPerTick / 3;
        }

        // reducing levitation bar while flying and increasing while not
        levitationCapacity += flying ? -levitationUsedPerTick : levitationRecoveryPerTick;
        // clamping capacity to not be less than 0 and being more than the capacity
        levitationCapacity = Mathf.Clamp(levitationCapacity, 0, maxLevitationCapacity);

        // updating levitation indicator
        radialLevitationIndicator.fillAmount = levitationCapacity / maxLevitationCapacity;

        bool newFacing = controller.Move(horizontalMove * Time.fixedDeltaTime, flying && levitationCapacity > 0, shooting);

        // TODO: un peu dégeu, faut retravailler ce morceau de code
        if(newFacing != facing)
        {
            if(dir.x < 0)
            {

                gun.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 180);
            } 
            else
            {
                gun.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            }
        }
        if (facing) 
        {
            if(dir.x < 0)
            {
                gun.localScale = new Vector3(-Mathf.Abs(gun.localScale.x), gun.localScale.y, gun.localScale.z);
                rotationOffset = 180;
            } 
            else
            {
                gun.localScale = new Vector3(Mathf.Abs(gun.localScale.x), gun.localScale.y, gun.localScale.z);
                rotationOffset = 0;
            }
        } 
        else
        {
            if (dir.x <= 0)
            {
                gun.localScale = new Vector3(Mathf.Abs(gun.localScale.x), gun.localScale.y, gun.localScale.z);
                rotationOffset = 0;
            }
            else
            {
                gun.localScale = new Vector3(-Mathf.Abs(gun.localScale.x), gun.localScale.y, gun.localScale.z);
                rotationOffset = 180;
            }
        }

        facing = newFacing;

        flying = false;
        shooting = false;
    }

}
