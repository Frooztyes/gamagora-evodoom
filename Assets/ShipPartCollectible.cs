using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPartCollectible : MonoBehaviour
{
    public enum ShipPart
    {
        ROCKET_LAUNCHERS,
        COCKPIT,
        LEFT_WING,
        RIGHT_WING
    };

    [SerializeField] public ShipPart shipPart;

    [SerializeField] private GameObject indicatorCollectible;

    // Start is called before the first frame update
    void Start()
    {
        indicatorCollectible.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            indicatorCollectible.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            indicatorCollectible.SetActive(false);
        }
    }
}
