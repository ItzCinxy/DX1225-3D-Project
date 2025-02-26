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

    // List to store objectives for the current map
    private List<Objective> currentObjectives;

    private bool objectivesCompleted = false;

    [Header("Map 1 Objectives")]
    [SerializeField] private Objective map1KeyObjective;
    [SerializeField] private Objective map1ZombieObjective;

    [Header("Map 2 Objectives")]
    //[SerializeField] private Objective map2BossObjective;
    [SerializeField] private Objective map2UnlockingArea;
    [SerializeField] private Objective map2ZombieObjective;

    private void Awake()
    {
        //if (Instance == null)
        //    Instance = this;
        //else
        //    Destroy(gameObject);

        //SetMapObjectives(1); // Initialize with map 1 objectives
    }

    void Start()
    {
        SetMapObjectives(1);

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private int currentMapIndex = 1;

    public void SetMapObjectives(int mapIndex)
    {
        objectivesCompleted = false;
        currentMapIndex = mapIndex;

        switch (mapIndex)
        {
            case 1:
                currentObjectives = new List<Objective> { map1KeyObjective, map1ZombieObjective };
                break;
            case 2:
                currentObjectives = new List<Objective> { map2ZombieObjective, map2UnlockingArea };
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
