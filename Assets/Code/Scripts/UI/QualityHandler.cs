using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class QualityHandler : OptionHandlerAb
{
    [SerializeField] private SaveSettings saveHandler;
    [SerializeField] private TextMeshProUGUI qualityText;

    private List<string> qualities = new()
    {
        "Very Low",
        "Low",
        "Medium",
        "High",
        "Very High",
        "Ultra", 
    };

    int currentQuality;

    // Start is called before the first frame update
    void Start()
    {
        int quality = saveHandler.sd.quality;
        SetQuality(quality);
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

        QualitySettings.SetQualityLevel(nextIndex);
        saveHandler.sd.quality = nextIndex;
        qualityText.text = qualities.ElementAt(nextIndex);
        currentQuality = nextIndex;
    }

    public override void DoAction(int index, int dir)
    {
        SetQuality(dir);
        saveHandler.PopulateSaveData(saveHandler.sd);
    }
}
