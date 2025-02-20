using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour, ISaveable
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider BGMSlider;
    [SerializeField] private Slider SFXSlider;

    private void Start()
    {
        if (PlayerPrefs.HasKey("BGMVolume"))
            Load();
        SetBGMMusicVol();

        if (PlayerPrefs.HasKey("SFXVolume"))
            Load();
        SetSFXMusicVol();
    }

    public void SetBGMMusicVol()
    {
        float bgmvol = BGMSlider.value;
        audioMixer.SetFloat("BGM", Mathf.Log10(bgmvol) * 20);
        PlayerPrefs.SetFloat("BGMVolume", bgmvol);
    }

    public void SetSFXMusicVol()
    {
        float sfxvol = SFXSlider.value;
        audioMixer.SetFloat("SFX", Mathf.Log10(sfxvol) * 20);
        PlayerPrefs.SetFloat("SFXVolume", sfxvol);
    }

    public void Save()
    {

    }

    public void Load()
    {
        BGMSlider.value = PlayerPrefs.GetFloat("BGMVolume");
        SetBGMMusicVol();
        SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        SetSFXMusicVol();
    }
}
