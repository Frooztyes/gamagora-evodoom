using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider effectsSlider;

    const string MIXER_MUSIC = "MusicVolume";
    const string MIXER_EFFECT = "EffectVolume";
    const string MIXER_MASTER = "MasterVolume";

    // Start is called before the first frame update
    void Start()
    {
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        effectsSlider.onValueChanged.AddListener(SetEffectVolume);
    }

    void SetMusicVolume(float value)
    {
        mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(value) * 20);
    }

    void SetMasterVolume(float value)
    {
        mixer.SetFloat(MIXER_MASTER, Mathf.Log10(value) * 20);
    }

    void SetEffectVolume(float value)
    {
        mixer.SetFloat(MIXER_EFFECT, Mathf.Log10(value) * 20);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
