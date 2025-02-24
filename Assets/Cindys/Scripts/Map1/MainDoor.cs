using UnityEngine;
using UnityEngine.SceneManagement;

public class MainDoor : MonoBehaviour
{
    private bool isUnlocked = false;

    public void UnlockDoor()
    {
        isUnlocked = true;
        Debug.Log("Door Unlocked! Press 'E' to Open.");
    }

    private void OnTriggerStay(Collider other)
    {
        if (isUnlocked && other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        SceneManager.LoadScene("Map2.1");
    }
}