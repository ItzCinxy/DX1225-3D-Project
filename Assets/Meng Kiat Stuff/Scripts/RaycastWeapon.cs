using Cinemachine;
using UnityEngine;

public class RaycastWeapon : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook _freeLookCamera;  // Reference to the Cinemachine FreeLook camera
    [SerializeField] private float shootRange = 100f; // Max shooting distance
    [SerializeField] private LayerMask targetLayer; // Set this to the "Target" layer in the Inspector

    private Vector3 lastRayOrigin; // Store the last ray origin for gizmos
    private Vector3 lastRayDirection; // Store the last ray direction for gizmos
    private bool hasShot = false; // Track if a shot has been fired

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) // Default Left Mouse Click
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("Main Camera is not assigned or missing!");
            return;
        }

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

    private void OnDrawGizmos()
    {
        if (!hasShot) return; // Only draw if a shot has been fired

        Gizmos.color = Color.red; // Color for the ray
        Gizmos.DrawRay(lastRayOrigin, lastRayDirection * shootRange); // Draw the ray
    }
}
