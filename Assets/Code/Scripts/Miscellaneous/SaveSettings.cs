using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSettings : MonoBehaviour, ISaveable
{
    public SaveData sd { get; private set; }

    public void LoadFromSaveData(SaveData saveData)
    {
        if(FileManager.LoadFromFile("SaveSettings.dat", out var json))
        {
            sd = new SaveData();
            sd.LoadFromJSON(json);
        }
    }

    public void PopulateSaveData(SaveData saveData)
    {
        if(FileManager.WriteToFile("SaveSettings.dat", sd.ToJSON())) {
            Debug.Log("Data saved");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        sd = new SaveData();
        LoadFromSaveData(sd);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
