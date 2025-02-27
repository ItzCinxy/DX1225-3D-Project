using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmChannel;
    public AudioSource sfxChannel;
    public AudioSource loopSfxChannel; // Separate channel for looping sounds like walking/running

    [Header("BGM Clips")]
    public List<AudioClip> bgmClips = new List<AudioClip>(); // Assign in Inspector

    [Header("Player SFX")]
    public AudioClip playerHurt;
    public AudioClip playerDie;
    public AudioClip playerJump;
    public AudioClip playerWalk;
    public AudioClip playerRun;

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (bgmChannel != null)
        {
            bgmChannel.loop = true; // Ensure BGM loops
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        AudioListener.pause = isPaused;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxChannel != null)
        {
            sfxChannel.PlayOneShot(clip);
        }
    }

    public void PlayLoopingSFX(AudioClip clip)
    {
        if (clip == null || loopSfxChannel == null) return;

        // Only play if the sound is not already playing
        if (!loopSfxChannel.isPlaying || loopSfxChannel.clip != clip)
        {
            loopSfxChannel.clip = clip;
            loopSfxChannel.loop = true;
            loopSfxChannel.Play();
        }
    }

    public void StopLoopingSFX()
    {
        if (loopSfxChannel != null)
        {
            loopSfxChannel.Stop();
        }
    }

    public bool IsPlaying(AudioClip clip)
    {
        return loopSfxChannel != null && loopSfxChannel.isPlaying && loopSfxChannel.clip == clip;
    }

    public void PlayBGM(int index)
    {
        if (bgmClips.Count == 0 || index >= bgmClips.Count) return;

        bgmChannel.clip = bgmClips[index];
        bgmChannel.Play();
    }
}