using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    private bool isUnlocked = false;
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
            Exit();
        }
    }

    private void Exit()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
