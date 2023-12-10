using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Load JSON file to read all user preferences for settings (FPS Limit, Resolution, etc.)
/// </summary>
public class SaveSettings : MonoBehaviour, ISaveable
{
    public SaveData SD { get; private set; }

    public void LoadFromSaveData(SaveData saveData)
    {
        if(FileManager.LoadFromFile("SaveSettings.dat", out var json))
        {
            SD = new SaveData();
            SD.LoadFromJSON(json);
        }
    }

    public void PopulateSaveData(SaveData saveData)
    {
        if(FileManager.WriteToFile("SaveSettings.dat", SD.ToJSON())) {
            Debug.Log("Data saved");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SD = new SaveData();
        LoadFromSaveData(SD);
    }
}
