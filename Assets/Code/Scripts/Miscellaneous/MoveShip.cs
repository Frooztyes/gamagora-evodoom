using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Randomly move the ship on the screen, only used on HUD, not a gameplay feature
/// </summary>
public class MoveShip : MonoBehaviour
{
    Vector3 nextPos;
    float speed;
    bool inCalculation = false;

    // Start is called before the first frame update
    void Start()
    {
        GetNextPos();
    }

    void GetNextPos()
    {
        int width = 1920;
        int height = 1080;
        // random position with a 10% borders on the screen
        nextPos.x = Random.Range(0.1f * width, 0.9f * width);
        nextPos.y = Random.Range(0.1f * height, 0.9f * height);
        speed = Random.Range(0.8f, 1.2f);
        inCalculation = false;
    }


    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, nextPos, Time.deltaTime * speed);
        if(Vector3.Distance(transform.position, nextPos) < 200 && !inCalculation)
        {
            inCalculation = true;
            Invoke(nameof(GetNextPos), Random.value * 4);
        }
    }
}
