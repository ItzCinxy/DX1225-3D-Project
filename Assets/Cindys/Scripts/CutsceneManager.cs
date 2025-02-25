using System;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    public PlayableDirector director; // Reference to a single PlayableDirector
    public PlayableAsset[] timelines; // Assign different Timelines in the Inspector

    private void Start()
    {
        PlayCutscene(1);
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