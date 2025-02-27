using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Zombie Spawning Settings")]
    public List<GameObject> zombiePrefabs; // List of zombie prefabs (drag in the Inspector)
    public int totalZombies = 350; // Total number of zombies to spawn
    public float spawnRadius = 50f; // Radius around the spawner to spawn zombies
    public float minSpawnDistance = 2f; // Minimum distance between zombies to prevent overlapping
    public LayerMask obstacleLayer; // Prevents spawning inside obstacles

    [Header("Spawning Optimization")]
    public bool spawnInWaves = false; // If true, zombies spawn over time instead of instantly
    public int zombiesPerWave = 50; // Number of zombies per wave
    public float waveInterval = 5f; // Time between waves

    private List<Vector3> usedSpawnPositions = new List<Vector3>(); // Stores used positions to prevent overlap

    void Awake()
    {
        if (spawnInWaves)
        {
            StartCoroutine(SpawnInWaves());
        }
        else
        {
            SpawnAllZombies();
        }
    }

    void SpawnAllZombies()
    {
        int zombiesSpawned = 0;
        while (zombiesSpawned < totalZombies)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();
            if (spawnPosition != Vector3.zero)
            {
                SpawnZombie(spawnPosition);
                zombiesSpawned++;
            }
        }
    }

    IEnumerator SpawnInWaves()
    {
        int zombiesSpawned = 0;
        while (zombiesSpawned < totalZombies)
        {
            for (int i = 0; i < zombiesPerWave && zombiesSpawned < totalZombies; i++)
            {
                Vector3 spawnPosition = GetValidSpawnPosition();
                if (spawnPosition != Vector3.zero)
                {
                    SpawnZombie(spawnPosition);
                    zombiesSpawned++;
                }
            }
            yield return new WaitForSeconds(waveInterval);
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        for (int i = 0; i < 10; i++) // Try 10 times to find a valid position
        {
            Vector3 randomPosition = transform.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0,
                Random.Range(-spawnRadius, spawnRadius)
            );

            // Check if position is inside obstacles
            if (!Physics.CheckSphere(randomPosition, minSpawnDistance, obstacleLayer))
            {
                // Ensure no other zombies are too close
                bool tooClose = false;
                foreach (Vector3 pos in usedSpawnPositions)
                {
                    if (Vector3.Distance(randomPosition, pos) < minSpawnDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    usedSpawnPositions.Add(randomPosition);
                    return randomPosition;
                }
            }
        }
        return Vector3.zero; // No valid position found
    }

    void SpawnZombie(Vector3 position)
    {
        if (zombiePrefabs.Count == 0) return; // No prefabs assigned

        GameObject zombiePrefab = zombiePrefabs[Random.Range(0, zombiePrefabs.Count)];
        Instantiate(zombiePrefab, position, Quaternion.identity);
    }
}
