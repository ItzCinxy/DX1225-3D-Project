using UnityEngine;

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
        Debug.Log("Door Opened! Proceed to next level.");
        gameObject.SetActive(false); // Hide the door
    }
}