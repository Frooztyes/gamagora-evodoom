using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CinematicShip : MonoBehaviour
{
    [SerializeField] FadeHandler fadePanel;
    [SerializeField] ScreenShake shake;


    [Header("Cinematic Part 1")]
    [SerializeField] Transform startingPosition;
    [SerializeField] Transform endingPosition;
    [SerializeField] private GameObject background;
    [SerializeField] private float moveToDuration = 5f;
    [Range(0, 1)]
    [SerializeField] private float percentStartFade = 0.8f;
    [SerializeField] AnimationCurve curve;
    [SerializeField] private float fadeDuration;
    [Range(0, 0.99999f)]
    [SerializeField] private float startMultiplierScale = 0;
    [SerializeField] private float endMultiplierScale = 1f;
    [SerializeField] AudioSource crashSound;
    [SerializeField] AudioSource heavyHitSound;

    [Header("Cinematic Part 2")]
    [SerializeField] Transform mudStartingPosition;
    [SerializeField] GameObject mudSoundEffect;
    [SerializeField] AudioSource alarmSound;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] List<GameObject> mudParticles;
    [Range(0, 1)]
    [SerializeField] private float percentAddMudSound = 0.8f;
    [SerializeField] private GameObject mudBackground;

    private Vector3 startScale;
    private Vector3 endScale;

    private Bounds shipSize;
    private List<AudioSource> mudSounds;
    CameraFollow mainCam;

    private void Start()
    {
        mudBackground.SetActive(false);
        transform.position = startingPosition.position;
        isAtRotation = false;
        mainCam = Camera.main.GetComponent<CameraFollow>();
        cinematicPart = 1;
        mudSounds = new List<AudioSource>();
        shipSize = transform.Find("GFX").GetComponent<SpriteRenderer>().bounds;
        shake.start = true;
        endScale = transform.localScale;
        moveToDuration = crashSound.clip.length/1.5f;
        transform.localScale *= startMultiplierScale;
        startScale = transform.localScale;
    }

    private float rotateElapsed = 0f;
    private float rotateDuration = 0.01f;

    private float moveToElapsed = 0f;
    private float moveToTime = 0f;

    bool isAtRotation;

    int cinematicPart;

    private void Update()
    {
        if(cinematicPart == 1)
        {
            if (!isAtRotation)
            {
                float angle = Mathf.Atan2(endingPosition.position.y - startingPosition.position.y, endingPosition.position.x - startingPosition.position.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
                if (targetRotation == transform.rotation)
                {
                    crashSound.Play();
                    isAtRotation = true;
                }
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateElapsed / rotateDuration);
                rotateElapsed += Time.deltaTime;
                return;
            }

            moveToTime += Time.deltaTime / moveToDuration;
            transform.position = Vector2.Lerp(startingPosition.position, endingPosition.position, curve.Evaluate(moveToTime));
            transform.localScale = Vector2.Lerp(startScale, endScale, curve.Evaluate(moveToTime));
            moveToElapsed += Time.deltaTime;

            if (!fadePanel.IsFading && Vector2.Distance(transform.position, startingPosition.position) >= Vector2.Distance(endingPosition.position, startingPosition.position) * percentStartFade)
            {
                fadePanel.IsFading = true;
                Invoke(nameof(FadeDelay), 0.4f);
                heavyHitSound.Play();
                reinvoke = 5;
                explosionOffset = endingPosition.position;
                multiplier = 2;
                AddExplosion();
                Invoke(nameof(SecondPart), fadeDuration + 0.5f);
            }
        }

        if(cinematicPart == 2)
        {
            foreach (AudioSource mudSound in mudSounds.ToList())
            {
                if (!mudSound.isPlaying)
                {
                    mudSounds.Remove(mudSound);
                    Destroy(mudSound.gameObject);
                }
            }
        }
    }

    private void FadeDelay()
    {
        fadePanel.Fade(fadeDuration);
    }

    private void SecondPart()
    {
        background.SetActive(false);
        mudBackground.SetActive(true);
        cinematicPart = 2;
        transform.localScale *= 2;
        transform.position = mudStartingPosition.position;
        fadePanel.UnFade(fadeDuration);
        alarmSound.Play();
        InvokeRepeating(nameof(AddMudSound), 0, mudSoundEffect.GetComponent<AudioSource>().clip.length * percentAddMudSound);
        reinvoke = -1;
        multiplier = 1;
        explosionOffset = mudStartingPosition.position;
        Invoke(nameof(AddExplosion), 0);
        foreach(GameObject particle in mudParticles)
        {
            particle.SetActive(true);
        }
    }

    private void AddMudSound()
    {
        AudioSource mudSound = Instantiate(mudSoundEffect, transform.position, Quaternion.identity).GetComponent<AudioSource>();
        mudSound.pitch = Random.Range(0.8f, 1.2f);
        mudSounds.Add(mudSound);
    }

    int reinvoke = -1;
    int multiplier = 1;
    Vector2 explosionOffset;

    private void AddExplosion()
    {
        Vector2 randomPosition = new Vector2(
            Random.Range(-shipSize.size.x/2, shipSize.size.x/2),
            Random.Range(-shipSize.size.y/2, 0)
        );
        randomPosition += (Vector2)explosionOffset;
        GameObject go = Instantiate(explosionPrefab, randomPosition, Quaternion.identity);
        go.transform.localScale = 50f * multiplier * Vector3.one;
        if(reinvoke > 0)
        {
            reinvoke--;
            Invoke(nameof(AddExplosion), 0.1f);
        } 
        else if(reinvoke == -1)
        {
            Invoke(nameof(AddExplosion), Random.Range(0.5f, 1.5f));
        }
    }



}
