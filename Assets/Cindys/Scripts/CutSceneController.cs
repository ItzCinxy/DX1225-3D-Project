using System;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    public PlayableDirector director; // Reference to a single PlayableDirector
    public PlayableAsset[] timelines; // Assign different Timelines in the Inspector

    public GameObject Zombie;

    [SerializeField] private GameObject PlayerUIpanel;

    public bool IsCutscenePlaying { get; private set; } = false; // Public read-only access

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

        PlayCutscene("StartGame");
    }

    public void PlayCutscene(string timelineName)
    {
        if (director == null || timelines == null || timelines.Length == 0)
        {
            Debug.LogWarning("CutsceneManager: Missing PlayableDirector or Timelines.");
            return;
        }

        // Find the timeline by name
        PlayableAsset selectedTimeline = null;
        foreach (var timeline in timelines)
        {
            if (timeline.name == timelineName)
            {
                selectedTimeline = timeline;
                break;
            }
        }

        if (selectedTimeline != null)
        {
            StopCutscene();
            director.playableAsset = selectedTimeline;
            IsCutscenePlaying = true; // Set flag to true
            Zombie.SetActive(false);

            if (PlayerUIpanel != null) PlayerUIpanel.SetActive(false);

            director.Play();
        }
        else
        {
            Debug.LogWarning($"CutsceneManager: No timeline found with name '{timelineName}'.");
        }
    }

    private void StopCutscene()
    {
        if (director != null && director.state == PlayState.Playing)
        {
            director.Stop();
            IsCutscenePlaying = false; // Reset flag when manually stopping
        }
    }

    private void OnCutsceneEnd(PlayableDirector pd)
    {
        IsCutscenePlaying = false; // Reset flag when the cutscene ends

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
