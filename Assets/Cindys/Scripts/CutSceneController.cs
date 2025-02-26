using System;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    public PlayableDirector director; // Reference to a single PlayableDirector
    public PlayableAsset[] timelines; // Assign different Timelines in the Inspector

    [SerializeField] private GameObject PlayerUIpanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (director != null)
        {
            director.stopped += OnCutsceneEnd;
        }

        PlayCutscene(0);
    }

    public void PlayCutscene(int index)
    {
        if (director == null || timelines == null || timelines.Length == 0)
        {
            Debug.LogWarning("CutsceneManager: Missing PlayableDirector or Timelines.");
            return;
        }

        if (index >= 0 && index < timelines.Length)
        {
            StopCutscene();
            director.playableAsset = timelines[index];

            if (PlayerUIpanel != null) PlayerUIpanel.SetActive(false);

            director.Play();
        }
        else
        {
            Debug.LogWarning($"CutsceneManager: Invalid timeline index {index}.");
        }
    }

    private void StopCutscene()
    {
        if (director != null && director.state == PlayState.Playing)
        {
            director.Stop();
        }
    }

    private void OnCutsceneEnd(PlayableDirector pd)
    {
        if (PlayerUIpanel != null) PlayerUIpanel.SetActive(true);
    }

    private void OnDestroy()
    {
        if (director != null)
        {
            director.stopped -= OnCutsceneEnd;
        }
    }
}