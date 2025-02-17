using Cinemachine;
using TMPro;
using UnityEngine;
using System.Collections; // Needed for Coroutine

public class Weapon : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook _freeLookCamera;  // Reference to the Cinemachine FreeLook camera
    [SerializeField] private float shootRange = 100f; // Max shooting distance
    [SerializeField] private LayerMask targetLayer; // Set this to the "Target" layer in the Inspector

    private Vector3 lastRayOrigin; // Store the last ray origin for gizmos
    private Vector3 lastRayDirection; // Store the last ray direction for gizmos
    private bool hasShot = false; // Track if a shot has been fired

    [Header("Ammo System")]
    [SerializeField] private int maxMagazineSize = 10;  // Max bullets per reload
    [SerializeField] private int totalAmmo = 100;       // Total ammo available
    private int currentAmmoInMag; // Bullets left in current mag

    [Header("Reload Settings")]
    [SerializeField] private float reloadTime = 2f; // Time in seconds to reload
    private bool isReloading = false;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text ammoDisplay;

    private void Start()
    {
        currentAmmoInMag = maxMagazineSize;
        UpdateAmmoDisplay();
    }

    void Update()
    {
        if (isReloading) return; // Prevent shooting while reloading

        if (Input.GetButtonDown("Fire1")) // Default Left Mouse Click
        {
            Shoot();
        }

        if (currentAmmoInMag <= 0 && totalAmmo > 0 && !isReloading)
        {
            Debug.Log("Out of ammo in mag! Reloading...");
            StartCoroutine(ReloadRoutine());
        }
    }

    private void Shoot()
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("Main Camera is not assigned or missing!");
            return;
        }

        if (currentAmmoInMag <= 0 || isReloading)
            return;

        currentAmmoInMag--;
        UpdateAmmoDisplay();
        Debug.Log($"Shots left in mag: {currentAmmoInMag} | Total ammo: {totalAmmo}");

        // Use the main camera's forward direction for aiming
        Transform camTransform = Camera.main.transform;
        Ray ray = new Ray(camTransform.position, camTransform.forward);

        lastRayOrigin = ray.origin; // Store for gizmos
        lastRayDirection = ray.direction; // Store for gizmos
        hasShot = true; // Indicate that a shot has been fired

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, shootRange, targetLayer)) // Check if we hit a target
        {
            Debug.Log($"Hit: {hit.collider.gameObject.name}");

            // Check if the hit object has a Target component
            Target target = hit.collider.GetComponent<Target>();
            if (target != null)
            {
                target.Hit(); // Call the Hit function on the target
            }
        }
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        ammoDisplay.text = "Reloading..."; // Update UI to show reloading

        yield return new WaitForSeconds(reloadTime); // Wait for reload time

        // Calculate how many bullets to reload
        int neededAmmo = maxMagazineSize - currentAmmoInMag;
        int ammoToReload = Mathf.Min(neededAmmo, totalAmmo); // Take only what's available

        if (ammoToReload > 0)
        {
            currentAmmoInMag += ammoToReload;
            totalAmmo -= ammoToReload;
        }

        isReloading = false;
        UpdateAmmoDisplay();
        Debug.Log("Reloaded!");
    }

    private void UpdateAmmoDisplay()
    {
        ammoDisplay.text = $"Ammo: {currentAmmoInMag} / {totalAmmo}";
    }

    private void OnDrawGizmos()
    {
        if (!hasShot) return; // Only draw if a shot has been fired

        Gizmos.color = Color.red; // Color for the ray
        Gizmos.DrawRay(lastRayOrigin, lastRayDirection * shootRange); // Draw the ray
    }
}