using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    private bool isUnlocked = false;
    private bool CutScenePlaying = false;
    public GameObject Zombie;

    public void UnlockDoor()
    {
        isUnlocked = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (isUnlocked && !CutScenePlaying && other.CompareTag("Player"))
        {
            CutScenePlaying = true;
            PlayCutscene();
        }
    }

    private void PlayCutscene()
    {
        CutsceneManager.Instance.PlayCutscene("EndingWinScene");
        Zombie.SetActive(false);

        //if (cutsceneDirector != null)
        //{
        //    cutsceneDirector.Play();
        //    //Invoke("Exit", (float)cutsceneDirector.duration); // Load scene after cutscene ends
        //}
        //else
        //{
        //    Debug.LogError("Cutscene Director is not assigned!");
        //}
    }
}
