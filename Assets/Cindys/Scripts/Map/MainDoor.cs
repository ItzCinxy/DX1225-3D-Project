using UnityEngine;

public class MainDoor : MonoBehaviour
{
    private bool isUnlocked = false;

    private void Update()
    {
        Debug.Log(isUnlocked);
    }

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
        SceneChanger.Instance.ChangeMap();
    }
}