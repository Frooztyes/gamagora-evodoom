using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeHandler : MonoBehaviour
{
    private Image panel;
    public bool IsFading { get; set; }

    private float fadeElapsed = 0f;
    private float fadeDuration;

    private Color fromColor;
    private Color toColor;

    private void Start()
    {
        panel = GetComponent<Image>();
        IsFading = false;
    }

    public void Fade(float duration)
    {
        fadeElapsed = 0;
        IsFading = true;
        fadeDuration = duration;
        fromColor = new Color(0f, 0f, 0f, 0f);
        toColor = Color.black;
    }

    public void UnFade(float duration)
    {
        fadeElapsed = 0;
        IsFading = true;
        fadeDuration = duration;
        fromColor = Color.black;
        toColor = new Color(0f, 0f, 0f, 0f);
    }

    private void Update()
    {
        if(IsFading)
        {
            panel.color = Color.Lerp(fromColor, toColor, fadeElapsed / fadeDuration);
            fadeElapsed += Time.deltaTime;
        }
    }


}
