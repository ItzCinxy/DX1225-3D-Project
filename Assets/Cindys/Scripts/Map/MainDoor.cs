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
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        SceneChanger.Instance.ChangeMap();
    }
}