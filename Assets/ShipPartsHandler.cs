using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipPartsHandler : MonoBehaviour
{
    [System.Serializable]
    private struct ShipSlot
    {
        public Sprite Greyed;
        public Sprite Active;

        public Color normal;
        public Color greyed;

        public Image slot;

        public void SetActive(bool status)
        {
            slot.sprite = status ? Active : Greyed;
            slot.color = status ? normal : greyed;
        }
    }

    [SerializeField] private ShipSlot rocketLauncher;
    [SerializeField] private ShipSlot leftWing;
    [SerializeField] private ShipSlot rightWing;
    [SerializeField] private ShipSlot cockpit;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetRocketPanel(bool active)
    {
        rocketLauncher.SetActive(active);
    }

    public void SetLeftWing(bool active)
    {
        leftWing.SetActive(active);
    }

    public void SetRightWing(bool active)
    {
        rightWing.SetActive(active);
    }

    public void SetCockpit(bool active)
    {
        cockpit.SetActive(active);
    }
}
