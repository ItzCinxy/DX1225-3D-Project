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
        Load();
    }

    public void SetBGMMusicVol()
    {
        float bgmvol = Mathf.Max(BGMSlider.value, 0.0001f); // Avoid log(0) issue
        audioMixer.SetFloat("BGM", Mathf.Log10(bgmvol) * 20);
        PlayerPrefs.SetFloat("BGMVolume", bgmvol);
        PlayerPrefs.Save(); // Ensure changes are saved
    }

    public void SetSFXMusicVol()
    {
        float sfxvol = Mathf.Max(SFXSlider.value, 0.0001f); // Avoid log(0) issue
        audioMixer.SetFloat("SFX", Mathf.Log10(sfxvol) * 20);
        PlayerPrefs.SetFloat("SFXVolume", sfxvol);
        PlayerPrefs.Save(); // Ensure changes are saved
    }

    public void Save()
    {
        PlayerPrefs.SetFloat("BGMVolume", BGMSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", SFXSlider.value);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey("BGMVolume"))
            BGMSlider.value = PlayerPrefs.GetFloat("BGMVolume");

        if (PlayerPrefs.HasKey("SFXVolume"))
            SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume");

        SetBGMMusicVol();
        SetSFXMusicVol();
    }
}