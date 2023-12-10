using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GameObject showing an enemmy or a ship part on the radar, disappear after a time
/// </summary>
public class RadarPing : MonoBehaviour
{
    private Image image;
    [SerializeField] private float disappearTimerMax = 1f;
    public Color color = Color.red;
    private float disappearTimer = 0f;
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        disappearTimer += Time.deltaTime;

        color.a = Mathf.Lerp(disappearTimerMax, 0f, disappearTimer / disappearTimerMax);
        image.color = color;

        if (disappearTimer >= disappearTimerMax)
        {
            Destroy(gameObject);
        }
    }

    public void SetDisappearTimer(float disappearTimer)
    {
        disappearTimerMax = disappearTimer;
        this.disappearTimer = 0f;
    }
}
