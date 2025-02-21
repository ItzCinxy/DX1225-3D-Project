using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Player")]
    public AudioSource playerChannel;
    public AudioClip playerHurt;
    public AudioClip playerDie;

    [Header("Weapon")]
    public AudioSource weaponChannel;
    public AudioClip weaponSwing;
    public AudioClip weaponBlock;

    [Header("Enemy Channel")]
    public AudioSource enemyChannel;
    public AudioClip enemyHurt;
    public AudioClip enemyDie;

    [Header("Audio Source")]
    public AudioSource bgmChannel;

    [Header("Overworld BGM (Plays when in empty areas)")]
    public AudioClip overworldBGM;

    [Header("Region BGMs (Set in Inspector)")]
    public List<RegionBGM> regionBGMs = new List<RegionBGM>();

    private Dictionary<string, AudioClip> regionBGM = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> regionBattleBGM = new Dictionary<string, AudioClip>();

    private string currentRegion = "";
    private bool isFighting = false;
    private bool isOutsideRegion = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (bgmChannel != null)
        {
            bgmChannel.loop = true;
        }

        regionBGM = new Dictionary<string, AudioClip>();
        regionBattleBGM = new Dictionary<string, AudioClip>();

        foreach (RegionBGM region in regionBGMs)
        {
            if (!regionBGM.ContainsKey(region.regionName))
            {
                regionBGM[region.regionName] = region.normalBGM;
            }
            if (!regionBattleBGM.ContainsKey(region.regionName))
            {
                regionBattleBGM[region.regionName] = region.battleBGM;
            }
        }
    }

    private void Start()
    {
        Debug.Log("Forcing Overworld BGM at game start...");
        PlayOverworldBGM();

        Debug.Log("Checking player's initial position...");
        CheckPlayerStartPosition();
    }

    private void CheckPlayerStartPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("No GameObject with tag 'Player' found in scene!");
            return;
        }

        Collider[] colliders = Physics.OverlapSphere(player.transform.position, 1f);
        foreach (Collider collider in colliders)
        {
            RegionTrigger regionTrigger = collider.GetComponent<RegionTrigger>();
            if (regionTrigger != null)
            {
                Debug.Log("Player spawned inside region: " + regionTrigger.regionName);
                ChangeRegion(regionTrigger.regionName);
                return;
            }
        }

        Debug.Log("Player spawned in empty area (Overworld). Playing Overworld BGM.");
        ResetToOverworldBGM();
    }

    public void ChangeRegion(string newRegion)
    {
        if (!regionBGM.ContainsKey(newRegion) || !regionBattleBGM.ContainsKey(newRegion))
        {
            Debug.LogWarning("Region BGM not set for: " + newRegion);
            return;
        }

        Debug.Log("Entering region: " + newRegion);

        currentRegion = newRegion;
        isOutsideRegion = false;

        if (!isFighting)
        {
            StartCoroutine(FadeAndSwitchBGM(regionBGM[newRegion]));
        }
    }

    public void SwitchToFightBGM()
    {
        if (!isFighting && regionBattleBGM.ContainsKey(currentRegion))
        {
            Debug.Log("Switching to Battle BGM in region: " + currentRegion);
            isFighting = true;
            StartCoroutine(FadeAndSwitchBGM(regionBattleBGM[currentRegion]));
        }
    }

    public void SwitchToNormalBGM()
    {
        if (isFighting && regionBGM.ContainsKey(currentRegion))
        {
            Debug.Log("Exiting Battle. Resuming Region BGM: " + currentRegion);
            isFighting = false;
            StartCoroutine(FadeAndSwitchBGM(regionBGM[currentRegion]));
        }
    }

    public void ResetToOverworldBGM()
    {
        if (isOutsideRegion) return;

        isOutsideRegion = true;
        currentRegion = "";

        PlayOverworldBGM();
    }

    private void PlayOverworldBGM()
    {
        if (overworldBGM != null)
        {
            Debug.Log("Overworld BGM is now playing: " + overworldBGM.name);

            bgmChannel.Stop();
            bgmChannel.clip = overworldBGM;
            bgmChannel.loop = true;
            bgmChannel.volume = 1f;
            bgmChannel.Play();

            Debug.Log("Is AudioSource Playing? " + bgmChannel.isPlaying);
        }
        else
        {
            Debug.Log("No Overworld BGM assigned, stopping music.");
            bgmChannel.Stop();
        }
    }

    public string GetCurrentRegion()
    {
        return currentRegion;
    }

    private IEnumerator FadeAndSwitchBGM(AudioClip newBgm)
    {
        if (bgmChannel.clip == newBgm)
        {
            Debug.Log("BGM already playing: " + newBgm.name);
            yield break;
        }

        Debug.Log("Switching BGM to: " + newBgm.name);
        float fadeDuration = 1.0f;
        float startVolume = bgmChannel.volume;

        while (bgmChannel.volume > 0)
        {
            bgmChannel.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        bgmChannel.clip = newBgm;
        bgmChannel.loop = true;
        bgmChannel.Play();

        while (bgmChannel.volume < startVolume)
        {
            bgmChannel.volume += startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        bgmChannel.volume = startVolume;
    }
}
