using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmChannel;
    public AudioSource sfxChannel;

    [Header("Player SFX")]
    public AudioClip playerHurt;
    public AudioClip playerDie;
    public AudioClip playerJump;

    private void Awake()
    {
        // Ensure only one instance of SoundManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxChannel != null)
        {
            sfxChannel.PlayOneShot(clip);
        }
    }


    private void Update()
    {

    }
}