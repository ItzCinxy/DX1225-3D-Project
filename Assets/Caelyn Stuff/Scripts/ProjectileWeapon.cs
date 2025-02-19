using UnityEngine;

public class ProjectileWeapon : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float launchForce = 20f;
    [SerializeField] private float upwardForce = 5f;

    public void Shoot()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError("Projectile prefab or fire point is missing!");
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        BoxCollider collider = projectile.GetComponent<BoxCollider>();

        if (rb) rb.isKinematic = false;
        if (collider) collider.enabled = true;

        if (rb != null)
        {
            Vector3 forceDirection = -(projectile.transform.forward * launchForce/* + firePoint.up * upwardForce*/);
            rb.AddForce(forceDirection, ForceMode.Impulse);
        }
    }
}
