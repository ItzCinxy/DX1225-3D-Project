using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
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
        float bgmvol = Mathf.Clamp(BGMSlider.value, 0.0001f, 20f);
        float normalizedValue = (bgmvol - 0.0001f) / (20f - 0.0001f);
        float dB = Mathf.Lerp(-80f, 0f, Mathf.Pow(normalizedValue, 0.5f));
        audioMixer.SetFloat("BGM", dB);
        PlayerPrefs.SetFloat("BGMVolume", bgmvol);
        PlayerPrefs.Save();
    }

    public void SetSFXMusicVol()
    {
        float sfxvol = Mathf.Clamp(SFXSlider.value, 0.0001f, 20f);
        float normalizedValue = (sfxvol - 0.0001f) / (20f - 0.0001f);
        float dB = Mathf.Lerp(-80f, 0f, Mathf.Pow(normalizedValue, 0.5f));
        audioMixer.SetFloat("SFX", dB);
        PlayerPrefs.SetFloat("SFXVolume", sfxvol);
        PlayerPrefs.Save();
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
        {
            BGMSlider.value = PlayerPrefs.GetFloat("BGMVolume");
        }
        else
        {
            BGMSlider.value = 1.0f;
            PlayerPrefs.SetFloat("BGMVolume", 1.0f);
            PlayerPrefs.Save();
        }

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        }
        else
        {
            SFXSlider.value = 1.0f;
            PlayerPrefs.SetFloat("SFXVolume", 1.0f);
            PlayerPrefs.Save();
        }

        // Apply volume settings
        SetBGMMusicVol();
        SetSFXMusicVol();
    }
}