using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private Transform Arrival;

    private void Start()
    {
        if(Arrival == null)
        {
            Destroy(transform.GetChild(2).gameObject);
            Destroy(transform.GetChild(1).gameObject);
            Destroy(transform.GetChild(0).gameObject);
            Destroy(GetComponent<BoxCollider2D>());
        } 
        else
        {
            Debug.Log(Arrival.transform.parent.name);
            GetComponentInChildren<TextMeshPro>().text = Arrival.transform.parent.name;
        }
        collided = null;
    }

    private GameObject collided;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && collided != null)
        {
            collided.transform.position = Arrival.position + (collided.GetComponent<SpriteRenderer>().bounds.size.y) * Vector3.up;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        collided = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collided = collision.gameObject;
    }
}
