using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieFlockManager : MonoBehaviour
{
    [SerializeField] private float neighborRadius = 5f;
    [SerializeField] private float separationWeight = 1f;
    [SerializeField] private float cohesionWeight = 0.5f;
    [SerializeField] private float alignmentWeight = 0.5f;

    private List<MonoBehaviour> allZombies = new List<MonoBehaviour>();

    void Start()
    {
        FindAllZombies();
        StartCoroutine(RefreshZombies()); // Automatically updates the list every few seconds
    }

    void Update()
    {
        ApplyFlocking();
    }

    void FindAllZombies()
    {
        allZombies.Clear();
        allZombies.AddRange(FindObjectsOfType<MonoBehaviour>());

        // Remove objects that aren't zombie AI controllers
        allZombies.RemoveAll(z => !(z is StandardZombieAIController || z is ChargerAIController ||
                                    z is SpitterZombieAIController || z is TankZombieAIController ||
                                    z is BomberZombieAIController));
    }

    public void RegisterZombie(MonoBehaviour newZombie)
    {
        if (!allZombies.Contains(newZombie))
        {
            allZombies.Add(newZombie);
        }
    }

    public void DeregisterZombie(MonoBehaviour zombie)
    {
        allZombies.Remove(zombie);
    }

    IEnumerator RefreshZombies()
    {
        while (true)
        {
            FindAllZombies();
            yield return new WaitForSeconds(5f); // Refresh every 5 seconds
        }
    }

    void ApplyFlocking()
    {
        foreach (MonoBehaviour zombie in allZombies)
        {
            if (zombie == null) continue; // Remove null entries (destroyed zombies)
            Vector3 flockingForce = ComputeFlocking(zombie.transform);
            zombie.GetComponent<Rigidbody>()?.AddForce(flockingForce);
        }
    }

    Vector3 ComputeFlocking(Transform zombie)
    {
        Vector3 separation = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        int neighborCount = 0;

        foreach (MonoBehaviour otherZombie in allZombies)
        {
            if (otherZombie == null || otherZombie.transform == zombie) continue;

            float distance = Vector3.Distance(zombie.position, otherZombie.transform.position);

            if (distance < neighborRadius)
            {
                separation += (zombie.position - otherZombie.transform.position).normalized / distance;
                cohesion += otherZombie.transform.position;

                Rigidbody otherRb = otherZombie.GetComponent<Rigidbody>();
                if (otherRb != null) alignment += otherRb.velocity;

                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            cohesion /= neighborCount;
            alignment /= neighborCount;

            cohesion = (cohesion - zombie.position).normalized;
            alignment = alignment.normalized;
        }

        return (separation * separationWeight) + (cohesion * cohesionWeight) + (alignment * alignmentWeight);
    }
}
