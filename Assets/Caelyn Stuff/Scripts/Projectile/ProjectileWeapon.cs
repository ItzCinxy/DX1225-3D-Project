using UnityEngine;
using System.Collections;

public class ProjectileWeapon : WeaponBase
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float launchForce = 20f;
    [SerializeField] private float upwardForce = 5f;
    [SerializeField] private LayerMask hitLayer;

    protected override void Start()
    {
        base.Start();
    }

    public override void Shoot()
    {
        if (isReloading || currentAmmoInMag <= 0)
        {
            Debug.Log("Out of ammo! Reload!");
            return;
        }

        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError("Projectile prefab or fire point is missing!");
            return;
        }

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        Vector3 targetPoint = Physics.Raycast(ray, out hit, 100f, hitLayer) ? hit.point : ray.origin + ray.direction * 100f;
        Vector3 direction = (targetPoint - firePoint.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        BoxCollider collider = projectile.GetComponent<BoxCollider>();

        if (collider) collider.enabled = true;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(direction * launchForce + Vector3.up * upwardForce, ForceMode.Impulse);
        }

        currentAmmoInMag--;
        UpdateAmmoDisplay();
    }
}