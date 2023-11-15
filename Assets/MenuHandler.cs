using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;
using static UnityEngine.Rendering.DebugUI;

public class MenuHandler : MonoBehaviour
{
    [SerializeField] Transform iconsContainer;

    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;
    [SerializeField] private float timeoutMax = 0.1f;
    [SerializeField] private AudioSource changeSound;
    [SerializeField] private AudioSource clickSound;
    [SerializeField] private Image canvasFade;


    [SerializeField] private SceneAsset gameScene;

    private Color fullBlack;
    private Color transparency;

    private float timeout = 0f;
    private CustomInputs input = null;

    private List<Image> icons;
    private int currentId;
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
    }

    private void OnDisable()
    {
        input.Disable();
        input.Menu.MenuActionMovement.performed -= OnChangePerformed;
        input.Menu.MenuActionPress.performed -= OnPressPerformed;
    }

    private void OnChangePerformed(InputAction.CallbackContext value)
    {
        // eviter les déplacements trop rapide
        if (timeout > 0f) return;
        timeout = timeoutMax;

        Vector2 valueInput = value.ReadValue<Vector2>();
        if(valueInput.y > 0)
        {
            SetButtonAt(currentId - 1);
        }
        else if(valueInput.y < 0)
        {
            SetButtonAt(currentId + 1);
        }
    }

    private void OnPressPerformed(InputAction.CallbackContext value)
    {
        if(value.ReadValue<float>() != 0)
        {
            clickSound.Play();
            ActionHandler();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        icons = new List<Image>();
        currentId = 0;

        foreach(Transform icon in iconsContainer)
        {
            icons.Add(icon.GetComponent<Image>());
        }
    }

    private bool isFading = false;

    void Fade()
    {
        isFading = true;
    }

    void SetGameScene()
    {
        SceneManager.LoadScene(gameScene.name);
    }

    void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    void ActionHandler()
    {
        switch (currentId)
        {
            case 0:
                Fade();
                Invoke(nameof(SetGameScene), lerpDuration + 0.5f);
                break;

            case 1:
                Debug.Log("Options");
                break;

            case 2:
                Debug.Log("Credits");
                break;

            case 3:
                Fade();
                Invoke(nameof(QuitApplication), lerpDuration + 0.5f);
                break;


            default:
                break;
        }

    }

    void SetButtonAt(int id)
    {
        if(id >= icons.Count)
        {
            id = 0;
        } 
        else if(id < 0)
        {
            id = icons.Count - 1;
        }

        icons[currentId].color = inactiveColor;
        icons[id].color = activeColor;
        currentId = id;
        changeSound.Play();
    }


    float fadeElapsed = 0f;
    float lerpDuration = 1.5f;

    // Update is called once per frame
    void Update()
    {
        if (timeout > -1)
        {
            timeout -= Time.deltaTime;
        }
        if(isFading)
        {
            canvasFade.color = Color.Lerp(new Color(0f, 0f, 0f, 0f), Color.black, fadeElapsed / lerpDuration);
            fadeElapsed += Time.deltaTime;
        }
    }
}
