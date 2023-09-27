using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class PlayerInputs : MonoBehaviour
{
    [Header("Statistics & Movements")]
    [SerializeField] private float runSpeed = 40f;
    [SerializeField] private float maxLevitationCapacity = 40f;
    private float defaultLevitationRecoveryPerTick;
    private float levitationCapacity;
    private float levitationRecoveryPerTick = 1;
    private readonly float levitationUsedPerTick = 1;
    private float horizontalMove = 0f;
    private bool flying;
    private Vector2 shootingDirection;

    [Header("Animations")]
    [SerializeField] private Image radialLevitationIndicator;
    [SerializeField] private Animator animator;
    // offset to rotate gun when being in reverse scale

    

    [Header("Miscellaneous")]
    [SerializeField] private MyCharacterController controller;

    // Start is called before the first frame update
    void Start()
    {
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
            Shoot();
        }

        // rotate toward wanted angle
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


    private void Shoot()
    {
        shootingDirection = (getMousePosition() - transform.position).normalized;
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

        controller.Move(horizontalMove * Time.fixedDeltaTime, flying && levitationCapacity > 0, shootingDirection);

        flying = false;
        shootingDirection = Vector2.zero;
    }

}
