using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class storing all values for player's preferences
/// </summary>
[System.Serializable]
public class SaveData
{
    // quality settings
    public int quality = 5;

    // video settings
    public Vector2 resolution = new(1920, 1080);
    public int frameRate = -1;
    public bool vsync = false;
    public FullScreenMode screenmode = FullScreenMode.FullScreenWindow;

    // audio settings
    public float masterVolume = 1;
    public float musicVolume = 1;
    public float effectVolume = 1;


    public string ToJSON()
    {
        return JsonUtility.ToJson(this);
    }

    public void LoadFromJSON(string aJSON)
    {
        JsonUtility.FromJsonOverwrite(aJSON, this);
    }
}

public interface ISaveable
{
    void PopulateSaveData(SaveData saveData);
    void LoadFromSaveData(SaveData saveData);
}
