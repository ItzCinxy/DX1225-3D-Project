using System;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    public PlayableDirector director; // Reference to a single PlayableDirector
    public PlayableAsset[] timelines; // Assign different Timelines in the Inspector

    private void Start()
    {
        PlayCutscene(0);

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);  // Ensure no duplicate instance exists
        }

    }

    public void PlayCutscene(int index)
    {
        if (index >= 0 && index < timelines.Length)
        {
            StopCutscene();
            director.playableAsset = timelines[index];
            director.Play();
        }
    }

    private void StopCutscene()
    {
        director.Stop();
    }
}