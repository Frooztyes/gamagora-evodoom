using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;


[RequireComponent(typeof(MyCharacterController))]
public class PlayerInputs : MonoBehaviour
{
    private CustomInputs input = null;
    private Vector2 moveVector = Vector2.zero;
    private float jumpValue = 0;

    private MyCharacterController controller;

    private bool shooting;
    private bool dodging;
    private bool reloading;

    private void Awake()
    {
        input = new CustomInputs();
    }
    private void FixedUpdate()
    {
        controller.Move(moveVector.x * Time.fixedDeltaTime, moveVector.y > 0 || jumpValue > 0f, shooting, dodging, reloading);
        shooting = false;
        dodging = false;
        reloading = false;
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Movement.performed += OnMovementPerformed;
        input.Player.Movement.canceled += OnMovementCancel;

        input.Player.Jump.performed += OnJumpPerformed;
        input.Player.Jump.canceled += OnJumpCanceled;

        input.Player.Shoot.performed += OnShootPerformed;
        input.Player.Shoot.canceled += OnShootCanceled;

        input.Player.Dodge.performed += OnDodgePerformed;
        input.Player.Dodge.canceled += OnDodgeCanceled;

        input.Player.Reload.performed += OnReloadingPerformed;
        input.Player.Reload.canceled += OnReloadingCanceled;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.Movement.performed -= OnMovementPerformed;
        input.Player.Movement.canceled  -= OnMovementCancel;

        input.Player.Jump.performed -= OnJumpPerformed;
        input.Player.Jump.canceled  -= OnJumpCanceled;

        input.Player.Shoot.performed -= OnShootPerformed;
        input.Player.Shoot.canceled  -= OnShootCanceled;

        input.Player.Dodge.performed -= OnDodgePerformed;
        input.Player.Dodge.canceled  -= OnDodgeCanceled;

        input.Player.Reload.performed -= OnReloadingPerformed;
        input.Player.Reload.canceled  -= OnReloadingCanceled;
    }

    // Start is called before the first frame update
    void Start() 
    {
        controller = GetComponent<MyCharacterController>();
    }

    private void OnMovementPerformed(InputAction.CallbackContext value)
    {
        moveVector = value.ReadValue<Vector2>();
    }

    private void OnMovementCancel(InputAction.CallbackContext value)
    {
        moveVector = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext value)
    {
        jumpValue = value.ReadValue<float>();
    }

    private void OnJumpCanceled(InputAction.CallbackContext value)
    {
        jumpValue = 0;
    }

    private void OnShootPerformed(InputAction.CallbackContext value)
    {
        shooting = (value.ReadValue<float>() > 0 ? true : false);
    }

    private void OnShootCanceled(InputAction.CallbackContext value)
    {
        shooting = false;
    }

    private void OnDodgePerformed(InputAction.CallbackContext value)
    {
        dodging = (value.ReadValue<float>() > 0 ? true : false);
    }

    private void OnDodgeCanceled(InputAction.CallbackContext value)
    {
        dodging = false;
    }

    private void OnReloadingPerformed(InputAction.CallbackContext value)
    {
        reloading = (value.ReadValue<float>() > 0 ? true : false);
    }

    private void OnReloadingCanceled(InputAction.CallbackContext value)
    {
        reloading = false;
    }

}
