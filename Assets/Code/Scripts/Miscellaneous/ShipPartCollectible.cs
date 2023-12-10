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

    public ShipPart shipPart;

    [SerializeField] private GameObject indicatorCollectible;

    // Start is called before the first frame update
    void Start()
    {
        GameObject indicator = GameObject.FindGameObjectWithTag("IndicatorCollectible");
        indicatorCollectible = indicator.transform.GetChild(0).gameObject;
        indicatorCollectible.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            indicatorCollectible.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            indicatorCollectible.SetActive(false);
        }
    }
}
