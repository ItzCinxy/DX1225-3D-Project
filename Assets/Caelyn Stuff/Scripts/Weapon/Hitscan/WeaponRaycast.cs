using UnityEngine;
using UnityEngine.VFX;

public class WeaponRaycast : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float shootRange = 100f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private int weaponDmg;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;

    private Vector3 lastRayOrigin;
    private Vector3 lastRayDirection;
    private bool hasShot = false;

    public void FireRaycast()
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("Main Camera is missing!");
            return;
        }

        Transform camTransform = Camera.main.transform;
        Ray ray = new Ray(camTransform.position, camTransform.forward);

        lastRayOrigin = ray.origin;
        lastRayDirection = ray.direction;
        hasShot = true;

        RaycastHit hit;
        // We cast to specific layers (targetLayer and other layers), but we check if the hit is valid.
        if (Physics.Raycast(ray, out hit, shootRange, targetLayer | LayerMask.GetMask("Default", "Environment", "Door", "Ground")))
        {
            // Check if we hit a valid target layer
            bool hitTargetLayer = ((targetLayer.value & (1 << hit.collider.gameObject.layer)) != 0);

            if (hitTargetLayer)
            {
                // Handle hit for different zombie AI types
                Target target = hit.collider.GetComponent<Target>();
                if (target != null)
                {
                    target.Hit();
                }

                StandardZombieAIController stdAI = hit.collider.GetComponentInChildren<StandardZombieAIController>();
                if (stdAI != null)
                {
                    stdAI.TakeDamage(weaponDmg);
                }

                TankZombieAIController tankAI = hit.collider.GetComponent<TankZombieAIController>();
                if (tankAI != null)
                {
                    tankAI.TakeDamage(weaponDmg);
                }

                BomberZombieAIController bmbAI = hit.collider.GetComponent<BomberZombieAIController>();
                if (bmbAI != null)
                {
                    bmbAI.TakeDamage(weaponDmg);
                }

                SpitterZombieAIController spitAI = hit.collider.GetComponent<SpitterZombieAIController>();
                if (spitAI != null)
                {
                    spitAI.TakeDamage(weaponDmg);
                }

                ChargerAIController chrgAI = hit.collider.GetComponent<ChargerAIController>();
                if (chrgAI != null)
                {
                    chrgAI.TakeDamage(weaponDmg);
                }

                // Instantiate VFX only when hitting a target layer
                if (hitEffectPrefab != null)
                {
                    GameObject vfxInstance = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));

                    // ✅ Get the Visual Effect component and Play it
                    VisualEffect vfx = vfxInstance.GetComponent<VisualEffect>();
                    if (vfx != null)
                    {
                        vfx.Play();
                    }

                    // ✅ Destroy the effect after its lifetime (e.g., 2 seconds)
                    Destroy(vfxInstance, 2f);
                }
            }
            // Optionally, if you want to handle the case where you hit the floor or non-target objects.
            else
            {
                Debug.Log("Hit something outside the target layer, no VFX.");
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (!hasShot) return;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(lastRayOrigin, lastRayDirection * shootRange);
    }
}
