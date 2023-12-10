using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LeavePlanet : MonoBehaviour
{
    CameraFollow cam;

    GameObject hud;

    float accel = 1;

    void Start()
    {
        hud = GameObject.FindGameObjectWithTag("HUD");
        cam = Camera.main.GetComponent<CameraFollow>();
        ScreenShake ss = Camera.main.GetComponent<ScreenShake>();
        ss.enabled = true;
        ss.start = true;
        cam.target = transform;
        hud.SetActive(false);
        Invoke(nameof(ChangeScene), 10f);
    }

    void ChangeScene()
    {
        SceneManager.LoadScene("EndMenu");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Translate(accel * Time.fixedDeltaTime * Vector3.up);
        accel += 0.1f;
    }
}
