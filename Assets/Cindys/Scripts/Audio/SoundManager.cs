using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmChannel;
    public AudioSource sfxChannel;

    [Header("Weapon Sounds")]
    public AudioClip[] shootingSounds;
    private float lastShootTime;
    private int shootSoundIndex = 0;
    public float resetTime = 1.0f;

    [Header("Player SFX")]
    public AudioClip playerHurt;
    public AudioClip playerDie;

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

    // General method to play SFX (can be used for any sound)
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxChannel != null)
        {
            sfxChannel.PlayOneShot(clip);
        }
    }

    // Play weapon shooting sounds directly to allow overlap
    public void PlayWeaponShootSound()
    {
        if (shootingSounds.Length == 0 || sfxChannel == null)
            return;

        // Play the current shooting sound directly
        sfxChannel.PlayOneShot(shootingSounds[shootSoundIndex]);

        // Move to the next sound, loop back after the last one
        shootSoundIndex = (shootSoundIndex + 1) % shootingSounds.Length;

        // Update the time of the last shot
        lastShootTime = Time.time;
    }

    private void Update()
    {
        // Reset the shooting sound index if no shots have been fired for the defined reset time
        if (Time.time - lastShootTime > resetTime)
        {
            shootSoundIndex = 0;
        }
    }
}