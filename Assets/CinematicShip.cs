using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CinematicShip : MonoBehaviour
{
    [SerializeField] FadeHandler fadePanel;

    [SerializeField] Transform startingPosition;
    [SerializeField] Transform endingPosition;

    [SerializeField] Transform mudStartingPosition;

    [SerializeField] private float moveToDuration = 5f;

    [Range(0, 1)]
    [SerializeField] private float percentStartFade = 0.8f;
    [SerializeField] AnimationCurve curve;
    [SerializeField] private float fadeDuration;

    CameraFollow mainCam;

    private void Start()
    {
        transform.position = startingPosition.position;
        isAtRotation = false;
        mainCam = Camera.main.GetComponent<CameraFollow>();
    }

    private float rotateElapsed = 0f;
    private float rotateDuration = 0.01f;

    private float moveToElapsed = 0f;
    private float moveToTime = 0f;

    bool isAtRotation;


    private void Update()
    {
        if(!isAtRotation)
        {
            float angle = Mathf.Atan2(endingPosition.position.y - startingPosition.position.y, endingPosition.position.x - startingPosition.position.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
            if (targetRotation == transform.rotation) isAtRotation = true;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateElapsed / rotateDuration);
            rotateElapsed += Time.deltaTime;
            return;
        }


        moveToTime += Time.deltaTime / moveToDuration;
        transform.position = Vector2.Lerp(startingPosition.position, endingPosition.position, curve.Evaluate(moveToTime));
        moveToElapsed += Time.deltaTime;

        if(!fadePanel.IsFading && Vector2.Distance(transform.position, startingPosition.position) > Vector2.Distance(endingPosition.position, startingPosition.position) * percentStartFade)
        {
            fadePanel.Fade(fadeDuration);
            Invoke(nameof(SecondPart), fadeDuration + 0.5f);
        }
    }

    private void SecondPart()
    {
        transform.position = mudStartingPosition.position;
    }


}
