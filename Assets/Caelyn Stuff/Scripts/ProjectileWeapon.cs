using System.Collections;
using TMPro;
using UnityEngine;

public class ProjectileWeapon : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float launchForce = 20f;
    [SerializeField] private float upwardForce = 5f;
    [SerializeField] private LayerMask hitLayer;

    [Header("Ammo System")]
    [SerializeField] private int maxMagazineSize = 6; 
    [SerializeField] private int totalAmmo = 30; 
    private int currentAmmoInMag;
    [SerializeField] private TMP_Text ammoDisplay;

    [Header("Reload Settings")]
    [SerializeField] private float reloadTime = 2f;
    private bool isReloading = false;

    private void Start()
    {
        currentAmmoInMag = maxMagazineSize; 
    }

    private void Update()
    {
        if (currentAmmoInMag <= 0)
            Reload();
    }

    public void Shoot()
    {
        if (isReloading) return;
        if (currentAmmoInMag <= 0)
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

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit, 100f, hitLayer))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 100f;
        }

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
        Debug.Log($"Shots left: {currentAmmoInMag}/{totalAmmo}");
    }

    public void Reload()
    {
        if (isReloading || totalAmmo <= 0) return;

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);

        int ammoToReload = Mathf.Min(maxMagazineSize - currentAmmoInMag, totalAmmo);
        currentAmmoInMag += ammoToReload;
        totalAmmo -= ammoToReload;

        Debug.Log($"Reloaded! {currentAmmoInMag}/{totalAmmo}");
        isReloading = false;
    }

    private void UpdateAmmoDisplay()
    {
        if (ammoDisplay != null)
            ammoDisplay.text = $"{currentAmmoInMag} / {totalAmmo}";
    }

    public void SetAmmoDisplay(TMP_Text newDisplay)
    {
        ammoDisplay = newDisplay;
        UpdateAmmoDisplay();
    }

    public void IncreaseTotalAmmo(int ammoInc)
    {
        totalAmmo += ammoInc;
        UpdateAmmoDisplay();
    }
}
