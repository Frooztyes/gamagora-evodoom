using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SwitchOptions : MonoBehaviour
{
    public enum Category
    {
        AUDIO, GRAPHICS, VIDEO
    };

    [Serializable]
    private struct ButtonCategory
    {
        public Image button;
        public GameObject category;
    }

    [SerializeField] private Color inactiveButton = Color.white;
    [SerializeField] private Color activeButton;
    [SerializeField] private List<ButtonCategory> buttons;
    [SerializeField] private GameObject menu;
    [SerializeField] private Image backIcon;

    private OptionHandlerAb currentCategory;

    [SerializeField] private AudioSource changeSound;
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;
    private List<Image> icons;

    private CustomInputs input = null;

    ButtonCategory currentActive;

    private void Awake()
    {
        input = new CustomInputs();
    }

    private void Start()
    {
        SetCategory(0);
    }

    private void OnEnable()
    {
        input.Enable();
        input.Menu.SwitchOptionsCategory.performed += OnSwitchPerformed;
        input.Menu.MenuActionMovement.performed += OnSwitchOptionPerformed;
        input.Menu.ChangeOptions.performed += OnChangeOptionPerformed;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Menu.SwitchOptionsCategory.performed -= OnSwitchPerformed;
        input.Menu.MenuActionMovement.performed -= OnSwitchOptionPerformed;
        input.Menu.ChangeOptions.performed -= OnChangeOptionPerformed;
    }

    private void OnChangeOptionPerformed(InputAction.CallbackContext value)
    {
        float offset = value.ReadValue<float>();
        if (offset > 0) offset = 1;
        else if (offset < 0) offset = -1;
        if (icons != null && currentCategory != null && currentId != icons.Count -1)
        {
            currentCategory.DoAction(currentId, (int)offset);
            changeSound.Play();
        } 
        if(currentId == icons.Count -1)
        {
            ReturnToMenu();
        }
    }

    private void OnSwitchOptionPerformed(InputAction.CallbackContext value)
    {
        float offset = value.ReadValue<float>();
        if (offset > 0) offset = 1;
        else if (offset < 0) offset = -1;
        SetButtonAt(currentId + (int) offset);
    }

    private void OnSwitchPerformed(InputAction.CallbackContext value)
    {
        float offset = value.ReadValue<float>();
        int index = buttons.IndexOf(currentActive) + (int) offset;
        if (index > buttons.Count - 1)
        {
            index = 0;
        }
        if (index < 0)
        {
            index = buttons.Count - 1;
        }

        SetCategory(index);
    }

    public void SetCategory(int category)
    {
        if (currentActive.button != null || currentActive.category != null)
        {
            currentActive.button.color = inactiveButton;
            currentActive.button.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
            currentActive.category.SetActive(false);
        }

        currentActive = buttons[category];
        currentActive.button.color = activeButton;
        currentActive.button.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        currentActive.category.SetActive(true);
        currentCategory = currentActive.category.GetComponent<OptionHandlerAb>();

        icons = new List<Image>();

        foreach (Transform icon in currentActive.category.transform.Find("Icons").transform)
        {
            Image im = icon.GetComponent<Image>();
            im.color = inactiveColor;
            icons.Add(im);
        }

        backIcon.color = inactiveColor;
        icons.Add(backIcon);

        currentId = 0;
        SetButtonAt(0);
    }

    int currentId = 0;

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

    public void ReturnToMenu()
    {
        gameObject.SetActive(false);
        menu.SetActive(true);
    }
}
