using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : OptionHandlerAb
{
    [SerializeField] private SaveSettings saveHandler;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider effectsSlider;

    const string MIXER_MUSIC = "MusicVolume";
    const string MIXER_EFFECT = "EffectVolume";
    const string MIXER_MASTER = "MasterVolume";

    void Start()
    {
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        effectsSlider.onValueChanged.AddListener(SetEffectVolume);

        float musicValue = saveHandler.sd.musicVolume;
        float effectValue = saveHandler.sd.effectVolume;
        float masterValue = saveHandler.sd.masterVolume;

        SetMasterVolume(masterValue);
        musicSlider.value = masterValue;

        SetMusicVolume(musicValue);
        effectsSlider.value = musicValue;

        SetEffectVolume(effectValue);
        masterSlider.value = effectValue;
    }

    void SetMusicVolume(float value)
    {
        mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(value) * 20);
        saveHandler.sd.musicVolume = value;
    }

    void SetMasterVolume(float value)
    {
        mixer.SetFloat(MIXER_MASTER, Mathf.Log10(value) * 20);
        saveHandler.sd.masterVolume = value;
    }

    void SetEffectVolume(float value)
    {
        mixer.SetFloat(MIXER_EFFECT, Mathf.Log10(value) * 20);
        saveHandler.sd.effectVolume = value;
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
        saveHandler.PopulateSaveData(saveHandler.sd);
    }
}
