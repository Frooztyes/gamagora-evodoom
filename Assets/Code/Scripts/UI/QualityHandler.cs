using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class QualityHandler : OptionHandlerAb
{
    [SerializeField] private TextMeshProUGUI qualityText;

    private Dictionary<string, int> qualities = new()
    {
        {"Very Low", 0},
        {"Low", 1},
        {"Medium", 2},
        {"High", 3},
        {"Very High", 4},
        {"Ultra", 5},
    };

    int currentQuality;

    // Start is called before the first frame update
    void Start()
    {
        SetQuality(qualities.Count - 1);
    }

    public void SetQuality(int offset)
    {
        int nextIndex = currentQuality + offset;
        if (nextIndex > qualities.Count - 1)
        {
            nextIndex = 0;
        }
        if (nextIndex < 0)
        {
            nextIndex = qualities.Count - 1;
        }

        QualitySettings.SetQualityLevel(qualities.ElementAt(nextIndex).Value);
        qualityText.text = qualities.ElementAt(nextIndex).Key;
        currentQuality = nextIndex;
    }

    public override void DoAction(int index, int dir)
    {
        SetQuality(dir);
    }
}
