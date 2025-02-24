using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;
    public GameObject mainDoor; // Assign the door in the Inspector

    private bool hasKey = false;
    private int zombiesKilled = 0;
    public int zombiesToKill = 5;
    private bool objectivesCompleted = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PickUpKey()
    {
        hasKey = true;
        CheckObjectives();
    }

    public void ZombieKilled()
    {
        zombiesKilled++;
        CheckObjectives();
    }

    private void CheckObjectives()
    {
        if (hasKey && zombiesKilled >= zombiesToKill && !objectivesCompleted)
        {
            objectivesCompleted = true;
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        if (mainDoor != null)
        {
            mainDoor.GetComponent<MainDoor>().UnlockDoor();
        }
    }

    private void Update()
    {
        Debug.Log("key" + hasKey);
        Debug.Log("zombie: " + zombiesKilled);
        Debug.Log("objective: " + objectivesCompleted);
    }
}