using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class PlayerInputs : MonoBehaviour
{
    [Header("Statistics & Movements")]
    [SerializeField] private Character character;

    [Header("Animations")]
    [SerializeField] private Image radialLevitationIndicator;
    [SerializeField] private Animator animator;

    [Header("Miscellaneous")]
    [SerializeField] private MyCharacterController controller;

    private float horizontalMove = 0f;
    private bool flying;
    private Vector2 shootingDirection;

    // Start is called before the first frame update
    void Start() {}

    // Update is called once per frame
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * character.RunSpeed;
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if(Input.GetButton("Jump"))
        {
            flying = true;
            animator.SetBool("IsJumping", true);
        }


        if (Input.GetMouseButtonDown(0))
        {
            shootingDirection = (getMousePosition() - transform.position).normalized;
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

    public void OnLanding()
    {
        animator.SetBool("IsJumping", false);
    }

    private void FixedUpdate()
    {
        character.UpdateLevitation(flying);

        // updating levitation indicator
        radialLevitationIndicator.fillAmount = character.GetLevitationFillAmount();

        controller.Move(horizontalMove * Time.fixedDeltaTime, character.GetJumpForce(flying), shootingDirection);

        flying = false;
        shootingDirection = Vector2.zero;
    }

}
