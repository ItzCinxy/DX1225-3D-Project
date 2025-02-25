using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Grenade Settings")]
    [SerializeField] private float explosionDelay = 3f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 700f;
    [SerializeField] private int explosionDamage = 50;
    [SerializeField] private GameObject explosionEffect;

    private Rigidbody rb;
    private bool hasExploded = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = false;

        Invoke("Explode", explosionDelay); // Start countdown
    }

    private void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;

        // Instantiate explosion effect
        if (explosionEffect != null)
        {
            GameObject explosionInstance = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosionInstance, explosionInstance.GetComponent<ParticleSystem>().main.duration);
        }

        // Apply explosion force to nearby objects
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearby in colliders)
        {
            Rigidbody rb = nearby.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            if (nearby.TryGetComponent(out StandardZombieAIController stdAI))
            {
                stdAI.TakeDamage(explosionDamage);
            }
            else if (nearby.TryGetComponent(out TankZombieAIController tankAI))
            {
                tankAI.TakeDamage(explosionDamage);
            }
            else if (nearby.TryGetComponent(out BomberZombieAIController bmbAI))
            {
                bmbAI.TakeDamage(explosionDamage);
            }
            //else if (nearby.TryGetComponent(out ScreamerZombieAIController scrmAI))
            //{
            //    scrmAI.TakeDamage(explosionDamage);
            //}
            else if (nearby.TryGetComponent(out ChargerAIController chrgAI))
            {
                chrgAI.TakeDamage(explosionDamage);
            }
        }

        Destroy(gameObject); // Destroy grenade after explosion
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasExploded)
        {
            Explode();
        }
    }
}