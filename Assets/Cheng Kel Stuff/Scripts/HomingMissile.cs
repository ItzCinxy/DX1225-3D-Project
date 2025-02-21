using UnityEngine;
using System;

public class HomingMissile : MonoBehaviour
{
    public float speed = 10f;
    public float rotationSpeed = 5f;
    public float explosionRadius = 2.5f;
    public GameObject explosionEffectPrefab;
    private Action<Vector3> explosionCallback; // Callback to apply damage

    public void SetExplosionCallback(Action<Vector3> callback)
    {
        explosionCallback = callback;
    }

    void Update()
    {
        Transform closestZombie = FindClosestZombie();

        if (closestZombie == null)
        {
            Destroy(gameObject); // Destroy missile if no target
            return;
        }

        // Move towards the closest zombie
        Vector3 direction = (closestZombie.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Rotate smoothly towards the target
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    Transform FindClosestZombie()
    {
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject zombie in zombies)
        {
            float distance = Vector3.Distance(transform.position, zombie.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = zombie.transform;
            }
        }

        return closest;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Zombie"))
        {
            Explode();
        }
    }

    void Explode()
    {
        if (explosionEffectPrefab != null)
        {
            GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

            Destroy(explosionEffect, 3f);
        }

        explosionCallback?.Invoke(transform.position);

        Destroy(gameObject);
    }

}
