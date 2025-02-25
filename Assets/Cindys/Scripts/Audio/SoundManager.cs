using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmChannel;
    public AudioSource sfxChannel;

    [Header("BGM Clips")]
    public List<AudioClip> bgmClips = new List<AudioClip>(); // Assign in Inspector

    [Header("Player SFX")]
    public AudioClip playerHurt;
    public AudioClip playerDie;
    public AudioClip playerJump;

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (bgmChannel != null)
        {
            bgmChannel.loop = true; // Ensure BGM loops
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            AudioListener.pause = true; // Pause all sounds
        }
        else
        {

            AudioListener.pause = false; // Resume sounds
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxChannel != null)
        {
            sfxChannel.PlayOneShot(clip);
        }
    }

    // Play a specific BGM (e.g., for a map)
    public void PlayBGM(int index)
    {
        if (bgmClips.Count == 0 || index >= bgmClips.Count) return;

        bgmChannel.clip = bgmClips[index];
        bgmChannel.Play();
    }
}