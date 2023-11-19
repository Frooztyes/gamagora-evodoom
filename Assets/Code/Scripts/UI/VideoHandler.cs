using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class VideoHandler : OptionHandlerAb
{

    [SerializeField] private List<Vector2> resolutions = new () 
    {
        new Vector2(1920, 1080),
        new Vector2(1366, 768),
        new Vector2(1280, 720),
        new Vector2(1024, 768),
        new Vector2(800, 600)
    };

    [SerializeField] private List<int> frameRates = new() 
    { 
        60, 
        75, 
        144, 
        240,
        -1
    };

    [SerializeField] private TextMeshProUGUI resolutionText;
    [SerializeField] private TextMeshProUGUI screenModeText;
    [SerializeField] private TextMeshProUGUI frameRateText;

    private readonly List<FullScreenMode> fullScreenModes = new()
    {
        FullScreenMode.Windowed,
        FullScreenMode.MaximizedWindow,
        FullScreenMode.FullScreenWindow
    };

    int currentResolution = 0;
    int currentFrameRate = 0;
    int currentScreenMode = 0;

    [SerializeField] private Toggle vsyncToggler;

    // Start is called before the first frame update
    void Start()
    {
        vsyncToggler.onValueChanged.AddListener(SetVSync);
        SetResolution(0);
        SetVSync(false);
        SetScreenMode(2);
        SetFrameRate(4);
    }

    private void SetVSync(bool value)
    {
        QualitySettings.vSyncCount = value ? 1 : 0;
    }

    public void SetResolution(int offset)
    {
        int nextIndex = currentResolution + offset;
        if (nextIndex > resolutions.Count - 1)
        {
            nextIndex = 0;
        }
        if (nextIndex < 0)
        {
            nextIndex = resolutions.Count - 1;
        }
        Screen.SetResolution((int)resolutions[nextIndex].x, (int)resolutions[nextIndex].y, fullScreenModes[currentScreenMode]);
        currentResolution = nextIndex;
        resolutionText.text = $"{(int)resolutions[nextIndex].x}x{(int)resolutions[nextIndex].y}";
    }

    public void SetScreenMode(int offset)
    {
        int nextIndex = currentScreenMode + offset;
        if (nextIndex > fullScreenModes.Count - 1)
        {
            nextIndex = 0;
        }
        if (nextIndex < 0)
        {
            nextIndex = fullScreenModes.Count - 1;
        }
        Screen.fullScreenMode = fullScreenModes[nextIndex];
        currentScreenMode = nextIndex;

        screenModeText.text = fullScreenModes[nextIndex] switch
        {
            FullScreenMode.FullScreenWindow => "Fullscreen",
            FullScreenMode.MaximizedWindow => "Borderless Windowed",
            FullScreenMode.Windowed => "Windowed",
            _ => "",
        };
    }

    public void SetFrameRate(int offset)
    {
        int nextIndex = currentFrameRate + offset;
        if (nextIndex > frameRates.Count - 1)
        {
            nextIndex = 0;
        }
        if (nextIndex < 0)
        {
            nextIndex = frameRates.Count - 1;
        }

        Application.targetFrameRate = frameRates[nextIndex];
        currentFrameRate = nextIndex;
        if (frameRates[nextIndex] == -1)
        {
            frameRateText.text = "Unlimited";
        }
        else
        {
            frameRateText.text = frameRates[nextIndex].ToString() + " FPS";
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public override void DoAction(int index, int dir)
    {
        switch (index)
        {
            case 0:
                SetResolution(dir);
                break;
            case 1:
                SetScreenMode(dir);
                break;
            case 2:
                SetFrameRate(dir);
                break;
            case 3:
                vsyncToggler.isOn = !vsyncToggler.isOn;
                SetVSync(vsyncToggler.isOn);
                break;

            default:
                break;
        }
    }
}
