using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

/// <summary>
/// Allow user to press A to return to menu
/// </summary>
public class EndHandler : MonoBehaviour
{
    private CustomInputs input = null;

    [SerializeField] private string menuScene;

    private void Awake()
    {
        Cursor.visible = false;
        input = new CustomInputs();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.InteractBis.performed += OnReturnToMenuPerformed;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.InteractBis.performed -= OnReturnToMenuPerformed;
    }

    private void OnReturnToMenuPerformed(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(menuScene);
    }
}
