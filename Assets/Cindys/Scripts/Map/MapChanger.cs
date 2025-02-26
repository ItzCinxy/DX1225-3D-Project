using System.Collections.Generic;
using UnityEngine;

public class MapChanger : MonoBehaviour
{
    public static MapChanger Instance { get; private set; }

    [SerializeField] private List<GameObject> maps; // List of maps
    [SerializeField] private Transform player;
    [SerializeField] private List<Vector3> spawnPositions; // List of player spawn positions

    private int currentMapIndex = 0;


    private void Start()
    {
        ActivateMap(currentMapIndex);

        player.position = spawnPositions[currentMapIndex];
        SoundManager.Instance.PlayBGM(0);

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeMap()
    {
        if (maps.Count == 0 || player == null || spawnPositions.Count == 0) return;

        maps[currentMapIndex].SetActive(false);

        currentMapIndex = (currentMapIndex + 1) % maps.Count;

        maps[currentMapIndex].SetActive(true);

        player.position = spawnPositions[currentMapIndex];

        SoundManager.Instance.PlayBGM(currentMapIndex);
        ObjectiveManager.Instance.SetMapObjectives(currentMapIndex + 1);
    }

    private void ActivateMap(int index)
    {
        foreach (GameObject map in maps)
        {
            map.SetActive(false);
        }

        if (maps.Count > 0)
        {
            maps[index].SetActive(true);
        }
    }
}