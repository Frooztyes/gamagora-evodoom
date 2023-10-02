using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
    private bool shooting;
    private Vector2 mousePosition;

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
            shooting = true;
        }
    }

    public void OnLanding()
    {
        animator.SetBool("IsJumping", false);
    }

    private void FixedUpdate()
    {
        character.UpdateLevitation(flying, controller.IsGrounded());

        // updating levitation indicator
        radialLevitationIndicator.fillAmount = character.GetLevitationFillAmount();

        controller.Move(horizontalMove * Time.fixedDeltaTime, character.GetJumpForce(flying), shooting);

        flying = false;
        shooting = false;
        mousePosition = Vector2.zero;
    }

}
