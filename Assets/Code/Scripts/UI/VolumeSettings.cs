using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class VolumeSettings : OptionHandlerAb
{
    [SerializeField] private SaveSettings saveHandler;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider effectsSlider;
    [SerializeField] private TextMeshProUGUI masterText;
    [SerializeField] private TextMeshProUGUI musicText;
    [SerializeField] private TextMeshProUGUI effectsText;

    const string MIXER_MUSIC = "MusicVolume";
    const string MIXER_EFFECT = "EffectVolume";
    const string MIXER_MASTER = "MasterVolume";

    void Start()
    {
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        effectsSlider.onValueChanged.AddListener(SetEffectVolume);

        float musicValue = saveHandler.SD.musicVolume;
        float effectValue = saveHandler.SD.effectVolume;
        float masterValue = saveHandler.SD.masterVolume;

        SetMasterVolume(masterValue);
        masterSlider.value = masterValue;
        masterValue *= 10;
        masterValue = Mathf.Round(masterValue * 10f) / 10;
        masterText.text = masterValue.ToString();

        SetMusicVolume(musicValue);
        musicSlider.value = musicValue;
        musicValue *= 10;
        musicValue = Mathf.Round(musicValue * 10f) / 10;
        musicText.text = musicValue.ToString();

        SetEffectVolume(effectValue);
        effectsSlider.value = effectValue;
        effectValue *= 10;
        effectValue = Mathf.Round(effectValue * 10f) / 10;
        effectsText.text = effectValue.ToString();
    }

    /// <summary>
    /// Change music volume setting
    /// </summary>
    /// <param name="value"></param>
    void SetMusicVolume(float value)
    {
        mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(value) * 20);
        saveHandler.SD.musicVolume = value;
        value *= 10;
        value = Mathf.Round(value * 10f) / 10;
        musicText.text = value.ToString();
    }

    /// <summary>
    /// Change master volume setting
    /// </summary>
    /// <param name="value"></param>
    void SetMasterVolume(float value)
    {
        mixer.SetFloat(MIXER_MASTER, Mathf.Log10(value) * 20);
        saveHandler.SD.masterVolume = value;
        value *= 10;
        value = Mathf.Round(value * 10f) / 10;
        masterText.text = value.ToString();
    }

    /// <summary>
    /// Change effect volume setting
    /// </summary>
    /// <param name="value"></param>
    void SetEffectVolume(float value)
    {
        mixer.SetFloat(MIXER_EFFECT, Mathf.Log10(value) * 20);
        saveHandler.SD.effectVolume = value;
        value *= 10;
        value = Mathf.Round(value * 10f) / 10;
        effectsText.text = value.ToString();
    }

    public override void DoAction(int index, int dir)
    {
        switch(index)
        {
            case 0:
                masterSlider.value += masterSlider.maxValue * 0.05f * dir;
                break;
            case 1:
                effectsSlider.value += effectsSlider.maxValue * 0.05f * dir;
                break;
            case 2:
                musicSlider.value += musicSlider.maxValue * 0.05f * dir;
                break;

            default:
                break;
        }
        saveHandler.PopulateSaveData(saveHandler.SD);
    }
}
