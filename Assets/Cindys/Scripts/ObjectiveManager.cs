using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    [Header("Main Door")]
    public GameObject mainDoor; // Assign the door in the Inspector

    [Header("Objective Displays")]
    [SerializeField] private TMP_Text questDisplay;

    // List to store objectives for the current map
    private List<Objective> currentObjectives;

    private bool objectivesCompleted = false;

    [Header("Map 1 Objectives")]
    [SerializeField] private Objective map1KeyObjective;
    [SerializeField] private Objective map1ZombieObjective;

    [Header("Map 2 Objectives")]
    [SerializeField] private Objective map2BossObjective;
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
        SetMapObjectives(1); // Initialize with map 1 objectives

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }

    public void SetMapObjectives(int mapIndex)
    {
        switch (mapIndex)
        {
            case 1:
                currentObjectives = new List<Objective> { map1KeyObjective, map1ZombieObjective };
                break;
            case 2:
                currentObjectives = new List<Objective> { map2BossObjective, map2ZombieObjective, map2UnlockingArea };
                break;
            default:
                currentObjectives = new List<Objective>();
                break;
        }

        UpdateObjectiveDisplay();
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
                    // For true/false objectives like collecting a key
                    questDisplay.text += $" {objective.objectiveName}: {(objective.isCompleted ? "Done" : "Not Done")} {status}\n";
                }
                else if (objective.type == ObjectiveType.Progress)
                {
                    // For count-based objectives like killing zombies
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
                objective.currentProgress++; // Increase count for this specific objective

                if (objective.currentProgress >= objective.totalRequired)
                {
                    objective.isCompleted = true;
                }
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
            questDisplay.text = "Objectives completed!\nDoor is opened.";
            OpenDoor();
        }
    }

    // Unlock the door when objectives are completed
    private void OpenDoor()
    {
        if (mainDoor != null)
        {
            mainDoor.GetComponent<MainDoor>().UnlockDoor();
        }
    }

}

public enum ObjectiveType
{
    Binary,  // Simple true/false objectives (e.g., collect key, unlock area)
    Progress // Count-based objectives (e.g., kill X zombies)
}

[System.Serializable]
public class Objective
{
    public string objectiveName;
    public ObjectiveType type; // Determines if it's a count-based or binary objective
    public bool isCompleted;

    // Only used for Progress-based objectives
    public int totalRequired;
    public int currentProgress;
}
