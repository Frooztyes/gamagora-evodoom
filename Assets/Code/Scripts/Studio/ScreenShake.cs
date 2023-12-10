using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public bool start = false;
    public bool stop = false;
    public AnimationCurve curve;
    private readonly float duration = float.PositiveInfinity;

    /// <summary>
    /// Move camera on a little offset to simulate a shake
    /// </summary>
    /// <returns></returns>
    IEnumerator Shaking()
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        while(elapsedTime < duration)
        {
            if (stop) break;
            elapsedTime += Time.deltaTime;
            transform.position = transform.position + Random.insideUnitSphere;
            yield return null;
        }
        transform.position = startPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(start)
        {
            stop = false;
            start = false;
            StartCoroutine(Shaking());
        }
    }
}
