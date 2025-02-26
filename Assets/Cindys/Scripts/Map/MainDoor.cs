using UnityEngine;

public class MainDoor : MonoBehaviour
{
    private bool isUnlocked = false;

    public void UnlockDoor()
    {
        isUnlocked = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (isUnlocked && other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            CutsceneManager.Instance.PlayCutscene("AfterMap1");
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        MapChanger.Instance.ChangeMap();
    }
}