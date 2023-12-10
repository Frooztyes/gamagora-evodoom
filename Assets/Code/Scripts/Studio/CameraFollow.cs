using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    private Vector3 offset = new(0, 0, -10f);
    private readonly float smoothTime = 0.15f;
    private Vector3 velocity = Vector3.zero;

    void FixedUpdate()
    {
        // Follow the target with a bit of smooth effect and with an offset
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
