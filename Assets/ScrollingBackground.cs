using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    private Vector2 length;
    private Vector2 startPos;
    [SerializeField] private float speed = 10f;
    [SerializeField] public Vector2 dir = Vector2.left;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        length = GetComponent<SpriteRenderer>().bounds.size;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(dir * Time.deltaTime * speed);

        if (transform.position.x < startPos.x - length.x)
        {
            transform.position = new Vector3(startPos.x, transform.position.y, transform.position.z);
        }

        if (transform.position.x > startPos.x + length.x)
        {
            transform.position = new Vector3(startPos.x, transform.position.y, transform.position.z);
        }


        if (transform.position.y < startPos.y - length.y)
        {
            transform.position = new Vector3(transform.position.x, startPos.y, transform.position.z);
        }

        if (transform.position.y > startPos.y + length.y)
        {
            transform.position = new Vector3(transform.position.x, startPos.y, transform.position.z);
        }
    }
}
