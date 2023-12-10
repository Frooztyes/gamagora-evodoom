using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RainbowColor : MonoBehaviour
{
    public float rainbowSpeed;
    public bool randomize;

    private float hue;
    private float sat;
    private float bri;
    private Image image;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        if(randomize)
        {
            hue = Random.Range(0f, 1f);
        }
        sat = 1;
        bri = 1;
        image.color = Color.HSVToRGB(hue, sat, bri);
    }

    // Update is called once per frame
    void Update()
    {
        Color.RGBToHSV(image.color, out hue, out sat, out bri);
        hue += rainbowSpeed / 1000;
        if(hue >= 1)
        {
            hue = 0;
        }
        image.color = Color.HSVToRGB(hue, sat, bri);
    }
}
