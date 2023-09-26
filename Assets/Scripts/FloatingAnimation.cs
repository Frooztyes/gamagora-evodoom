using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingAnimation : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float height;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(transform.position.y);
        //float newPos = Mathf.Sin(Time.time * speed) * height;
        ////Debug.Log(newPos);
        //transform.position = new Vector3(transform.position.x, newPos, transform.position.z);
    }
}
