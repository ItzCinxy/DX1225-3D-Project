using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    private bool isUnlocked = false;
    public PlayableDirector cutsceneDirector; // Reference to the Timeline cutscene
    private void Update()
    {
        Debug.Log(isUnlocked);
    }
    public void UnlockDoor()
    {
        isUnlocked = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (isUnlocked && other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            PlayCutscene();
        }
    }

    private void PlayCutscene()
    {
        if (cutsceneDirector != null)
        {
            cutsceneDirector.Play();
            Invoke("Exit", (float)cutsceneDirector.duration); // Load scene after cutscene ends
        }
        else
        {
            Debug.LogError("Cutscene Director is not assigned!");
        }
    }
}
