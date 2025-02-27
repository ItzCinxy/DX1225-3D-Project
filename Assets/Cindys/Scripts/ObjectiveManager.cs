using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    [Header("Main Doors")]
    public GameObject Map1MainDoor;
    public GameObject Map2MainDoor;

    [Header("Objective Displays")]
    [SerializeField] private TMP_Text questDisplay;

    [Header("Objective Trail")]
    [SerializeField] private ObjectiveTrail objectiveTrail;
    [SerializeField] private Transform playerStartPosition; // Player's spawn point

    // List to store objectives for the current map
    private List<Objective> currentObjectives;

    private bool objectivesCompleted = false;

    [Header("Map 1 Objectives")]
    [SerializeField] private Objective map1KeyObjective;
    [SerializeField] private Objective map1ZombieObjective;
    [SerializeField] private Transform map1KeyLocation; // Location of the keycard
    [SerializeField] private Transform map1DoorLocation; // Location of the main door

    [Header("Map 2 Objectives")]
    //[SerializeField] private Objective map2BossObjective;
    [SerializeField] private Objective map2UnlockingArea;
    [SerializeField] private Objective map2ZombieObjective;
    [SerializeField] private Transform map2UnlockLocation; // Location to unlock

    void Start()
    {
        SetMapObjectives(1);

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private int currentMapIndex = 1;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) { ReactivateTrail(); }
    }

    public void SetMapObjectives(int mapIndex)
    {
        objectivesCompleted = false;
        currentMapIndex = mapIndex;

        switch (mapIndex)
        {
            case 1:
                currentObjectives = new List<Objective> { map1KeyObjective, map1ZombieObjective };
                if (!map1KeyObjective.isCompleted)
                    objectiveTrail.SetTarget(map1KeyLocation, playerStartPosition); // Guide to keycard
                break;
            case 2:
                currentObjectives = new List<Objective> { map2ZombieObjective, map2UnlockingArea };
                if (!map2UnlockingArea.isCompleted)
                    objectiveTrail.SetTarget(map2UnlockLocation, playerStartPosition); // Guide to unlocking area
                break;
            default:
                currentObjectives = new List<Objective>();
                break;
        }

        UpdateObjectiveDisplay();
    }

    private int GetCurrentMapIndex()
    {
        return currentMapIndex;
    }

    private void UpdateObjectiveDisplay()
    {
        if (questDisplay != null)
        {
            questDisplay.text = "";
            foreach (var objective in currentObjectives)
            {
                string status = objective.isCompleted ? "(Completed)" : "(In Progress)";

                if (objective.type == ObjectiveType.Binary)
                {
                    questDisplay.text += $" {objective.objectiveName}: {(objective.isCompleted ? "Done" : "Not Done")} {status}\n";
                }
                else if (objective.type == ObjectiveType.Progress)
                {
                    questDisplay.text += $" {objective.objectiveName}: {objective.currentProgress} / {objective.totalRequired} {status}\n";
                }
            }
        }
    }


    public void PickUpKey()
    {
        if (!objectivesCompleted)
        {
            foreach (var objective in currentObjectives)
            {
                if (objective.type == ObjectiveType.Binary && objective.objectiveName.ToLower().Contains("key"))
                {
                    objective.isCompleted = true;
                }
            }

            UpdateObjectiveDisplay();
            CheckObjectives();

            objectiveTrail.SetTarget(map1DoorLocation, playerStartPosition);
        }
    }

    public void ZombieKilled()
    {
        if (objectivesCompleted) return;

        foreach (var objective in currentObjectives)
        {
            if (objective.type == ObjectiveType.Progress && objective.objectiveName.ToLower().Contains("zombie") && !objective.isCompleted)
            {
                Debug.Log($"Updating objective: {objective.objectiveName}");

                objective.currentProgress++; // Increase count for this specific objective

                if (objective.currentProgress >= objective.totalRequired)
                {
                    objective.isCompleted = true;
                    Debug.Log($"Objective completed: {objective.objectiveName}");
                }
            }
        }

        UpdateObjectiveDisplay();
        CheckObjectives();
    }

    public void BossKilled()
    {
        if (objectivesCompleted) return;

        foreach (var objective in currentObjectives)
        {
            if (objective.type == ObjectiveType.Binary && objective.objectiveName.ToLower().Contains("boss") && !objective.isCompleted)
            {
                objective.isCompleted = true;
            }
        }

        UpdateObjectiveDisplay();
        CheckObjectives();
    }

    public void UnlockArea()
    {
        if (objectivesCompleted) return;

        foreach (var objective in currentObjectives)
        {
            if (objective.type == ObjectiveType.Binary && objective.objectiveName.ToLower().Contains("area") && !objective.isCompleted)
            {
                objective.isCompleted = true;
            }
        }

        UpdateObjectiveDisplay();
        CheckObjectives();

        objectiveTrail.DisableTrail();
    }

    private void ReactivateTrail()
    {
        if (objectivesCompleted)
        {
            objectiveTrail.DisableTrail(); // Don't show if everything is done
            return;
        }

        foreach (var objective in currentObjectives)
        {
            if (!objective.isCompleted) // Find the next unfinished objective
            {
                if (objective.objectiveName.ToLower().Contains("key"))
                {
                    objectiveTrail.SetTarget(map1KeyLocation, playerStartPosition);
                    return;
                }
                if (objective.objectiveName.ToLower().Contains("door"))
                {
                    objectiveTrail.SetTarget(map1DoorLocation, playerStartPosition);
                    return;
                }
                if (objective.objectiveName.ToLower().Contains("area"))
                {
                    objectiveTrail.SetTarget(map2UnlockLocation, playerStartPosition);
                    return;
                }
            }
        }
    }

    private void CheckObjectives()
    {
        bool allCompleted = true;
        foreach (var obj in currentObjectives)
        {
            if (!obj.isCompleted)
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted && !objectivesCompleted)
        {
            objectivesCompleted = true;

            if (currentMapIndex == 1)
            {
                questDisplay.text = "Objectives completed!\nDoor is opened.";
            }
            else if (currentMapIndex == 2)
            {
                questDisplay.text = "Objectives completed!\nRun to the train station.";
            }

            OpenDoor(currentMapIndex); // Pass the current map index to the OpenDoor method

            objectiveTrail.DisableTrail();
        }
    }

    private void OpenDoor(int mapIndex)
    {
        if (!objectivesCompleted) return; // Ensure objectives are actually completed before unlocking

        switch (mapIndex)
        {
            case 1:
                if (Map1MainDoor != null)
                {
                    Debug.Log("Unlocking Map 1 Main Door");
                    Map1MainDoor.GetComponent<MainDoor>().UnlockDoor();
                }
                break;

            case 2:
                if (Map2MainDoor != null)
                {
                    Debug.Log("Unlocking Map 2 Main Door");
                    Map2MainDoor.GetComponent<EndGame>().UnlockDoor();
                }
                break;

            default:
                Debug.LogWarning("Invalid map index for OpenDoor()");
                break;
        }
    }


}

public enum ObjectiveType
{
    Binary,
    Progress
}

[System.Serializable]
public class Objective
{
    public string objectiveName;
    public ObjectiveType type; 
    public bool isCompleted;

    public int totalRequired;
    public int currentProgress;
}
