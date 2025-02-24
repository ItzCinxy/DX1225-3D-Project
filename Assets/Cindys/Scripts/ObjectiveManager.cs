using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;
    public GameObject mainDoor; // Assign the door in the Inspector
    [SerializeField] private TMP_Text questKeyDisplay;
    [SerializeField] private TMP_Text questZombieDisplay;

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

        if (questKeyDisplay != null) questKeyDisplay.text = $"Key collected: {hasKey}";
        if (questZombieDisplay != null) questZombieDisplay.text = $"Zombies killed: {zombiesKilled} / {zombiesToKill}";
    }

    public void PickUpKey()
    {
        hasKey = true;
        questKeyDisplay.text = $"Key collected: {hasKey}";
        CheckObjectives();
    }

    public void ZombieKilled()
    {
        zombiesKilled++;
        questZombieDisplay.text = $"Zombies killed: {zombiesKilled} / {zombiesToKill}";
        CheckObjectives();
    }

    private void CheckObjectives()
    {
        if (hasKey && zombiesKilled >= zombiesToKill && !objectivesCompleted)
        {
            objectivesCompleted = true;
            questKeyDisplay.text = "Objectives completed!\n Door is opened.";
            questZombieDisplay.text = "";
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