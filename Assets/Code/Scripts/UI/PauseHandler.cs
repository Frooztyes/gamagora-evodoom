using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;

public class PauseHandler : MonoBehaviour
{
    public PlayerInputs playerInput { get; set; }

    [SerializeField] Transform iconsContainer;

    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;
    [SerializeField] private AudioSource changeSound;
    [SerializeField] private AudioSource clickSound;
    [SerializeField] private GameObject options;

    [SerializeField] private FadeHandler fadeHandler;
    [SerializeField] private string mainMenuSceneName;


    private List<Image> icons;
    private int currentId;

    private CustomInputs input = null;
    private float fadeDuration = 0.5f;

    private void Start()
    {
        icons = new List<Image>();
        currentId = 0;

        foreach (Transform icon in iconsContainer)
        {
            icons.Add(icon.GetComponent<Image>());
        }
    }
    private void Awake()
    {
        Cursor.visible = false;
        input = new CustomInputs();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Menu.MenuActionMovement.performed += OnChangePerformed;
        input.Menu.MenuActionPress.performed += OnPressPerformed;
        input.Player.Pause.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Menu.MenuActionMovement.performed -= OnChangePerformed;
        input.Menu.MenuActionPress.performed -= OnPressPerformed;
        input.Player.Pause.performed -= OnPausePerformed;
    }
    private void OnPausePerformed(InputAction.CallbackContext value)
    {
        Resume();
    }

    private void OnChangePerformed(InputAction.CallbackContext value)
    {
        float offset = value.ReadValue<float>();
        if (offset > 0) offset = 1;
        else if (offset < 0) offset = -1;
        SetButtonAt(currentId + (int)offset);
    }

    private void OnPressPerformed(InputAction.CallbackContext value)
    {
        if (value.ReadValue<float>() != 0)
        {
            clickSound.Play();
            ActionHandler();
        }
    }
    void ActionHandler()
    {
        switch (currentId)
        {
            case 0:
                Resume();
                break;

            case 1:
                OptionMenu();
                break;

            case 2:
                ReturnToMenu();
                break;

            default:
                break;
        }

    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
        Time.timeScale = 1;
    }

    void OptionMenu()
    {
        gameObject.SetActive(false);
        options.SetActive(true);
    }

    void Resume()
    {
        gameObject.SetActive(false);
        playerInput.enabled = true;
        Time.timeScale = 1;
    }

    void SetButtonAt(int id)
    {
        if (id >= icons.Count)
        {
            id = 0;
        }
        else if (id < 0)
        {
            id = icons.Count - 1;
        }

        icons[currentId].color = inactiveColor;
        icons[id].color = activeColor;
        currentId = id;
        changeSound.Play();
    }
}
