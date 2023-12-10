using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handle ship part retreiving by updating HUD
/// </summary>
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
    [SerializeField] private GameObject IndicatorLeave;

    int partRetreived = 0;

    // Update is called once per frame
    void Update()
    {
        // if all pieces are retreives, show that the player can leave 
        if(partRetreived >= 4 && !IndicatorLeave.activeSelf)
        {
            IndicatorLeave.SetActive(true);
        }
    }

    public bool SetRocketPanel(bool active)
    {
        rocketLauncher.SetActive(active);
        partRetreived++;
        return partRetreived >= 4;
    }

    public bool SetLeftWing(bool active)
    {
        leftWing.SetActive(active);
        partRetreived++;
        return partRetreived >= 4;
    }

    public bool SetRightWing(bool active)
    {
        rightWing.SetActive(active);
        partRetreived++;
        return partRetreived >= 4;
    }

    public bool SetCockpit(bool active)
    {
        cockpit.SetActive(active);
        partRetreived++;
        return partRetreived >= 4;
    }
}
