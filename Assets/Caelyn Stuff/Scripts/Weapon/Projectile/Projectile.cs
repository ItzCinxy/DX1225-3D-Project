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

            float distance = Vector3.Distance(transform.position, nearby.transform.position);
            float damageMultiplier = Mathf.Clamp01(1 - (distance / explosionRadius)); // Closer = More Damage
            int finalDamage = Mathf.RoundToInt(explosionDamage * damageMultiplier);

            if (nearby.TryGetComponent(out StandardZombieAIController stdAI))
                stdAI.TakeDamage(finalDamage);
            else if (nearby.TryGetComponent(out TankZombieAIController tankAI))
                tankAI.TakeDamage(finalDamage);
            else if (nearby.TryGetComponent(out BomberZombieAIController bmbAI))
                bmbAI.TakeDamage(finalDamage);
            //else if (nearby.TryGetComponent(out ScreamerZombieAIController scrmAI))
            //    scrmAI.TakeDamage(finalDamage);
            else if (nearby.TryGetComponent(out SpitterZombieAIController spitAI))
                spitAI.TakeDamage(finalDamage);
            else if (nearby.TryGetComponent(out ChargerAIController chrgAI))
                chrgAI.TakeDamage(finalDamage);
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