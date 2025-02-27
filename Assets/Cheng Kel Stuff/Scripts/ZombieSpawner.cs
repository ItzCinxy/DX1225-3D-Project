using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Zombie Spawning Settings")]
    public List<GameObject> zombiePrefabs; // List of zombie prefabs
    public int totalZombies = 365; // Total number of zombies to spawn
    public float spawnRadius = 50f; // Radius around the spawner
    public float minSpawnDistance = 2f; // Prevents overlapping
    public LayerMask obstacleLayer; // Prevent spawning in walls

    [Header("Spawning Optimization")]
    public bool spawnInWaves = false;
    public int zombiesPerWave = 50;
    public float waveInterval = 60f;

    public GameObject zombieParent;

    private List<Vector3> usedSpawnPositions = new List<Vector3>();

    [SerializeField] private GameObject fog;

    private void Awake()
    {
        StartCoroutine(WaitForMapChangeAndSpawn());
    }

    IEnumerator WaitForMapChangeAndSpawn()
    {
        // Wait until `MapChanger` finishes changing maps
        while (MapChanger.Instance == null)
        {
            yield return null; // Wait a frame
        }

        yield return new WaitForSeconds(0.85f); // Small delay to ensure the map loads

        if (spawnInWaves)
        {
            StartCoroutine(SpawnInWaves());
        }
        else
        {
            SpawnAllZombies();
        }

        fog.SetActive(true);
        ParticleSystem fogParticle = fog.GetComponent<ParticleSystem>();
        fogParticle.Play();
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
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPosition = transform.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0,
                Random.Range(-spawnRadius, spawnRadius)
            );

            if (!Physics.CheckSphere(randomPosition, minSpawnDistance, obstacleLayer))
            {
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
        return Vector3.zero;
    }

    void SpawnZombie(Vector3 position)
    {
        if (zombiePrefabs.Count == 0) return;

        // Get a random zombie prefab
        GameObject zombiePrefab = zombiePrefabs[Random.Range(0, zombiePrefabs.Count)];

        // Find or create the "Zombie" parent object
        zombieParent = GameObject.Find("ZombieMap2");
        if (zombieParent == null)
        {
            zombieParent = new GameObject("ZombieMap2"); // Create if it doesn’t exist
        }

        // Instantiate zombie and set parent
        GameObject newZombie = Instantiate(zombiePrefab, position, Quaternion.identity);
        newZombie.transform.parent = zombieParent.transform; // Set parent to "Zombie"
    }

}
