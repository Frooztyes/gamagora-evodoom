using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
        nextPos.x = Random.Range(0.1f * Screen.width, 0.9f * Screen.width);
        nextPos.y = Random.Range(0.1f * Screen.height, 0.9f * Screen.height);
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
