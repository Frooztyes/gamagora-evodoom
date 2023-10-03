using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipGameObject : MonoBehaviour
{
    [SerializeField] private GameObject attack;
    [SerializeField] private GameObject direction;

    private int hitpoints = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hitpoints <= 0) return;
        hitpoints--;
        attack.transform.localScale = new Vector3(-attack.transform.localScale.x, attack.transform.localScale.y, attack.transform.localScale.z);
        direction.transform.localScale = new Vector3(direction.transform.localScale.x, -direction.transform.localScale.y, direction.transform.localScale.z);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        hitpoints = 1;
    }
}
