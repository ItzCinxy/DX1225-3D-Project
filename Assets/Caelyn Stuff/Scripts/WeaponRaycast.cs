using UnityEngine;

public class WeaponRaycast : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float shootRange = 100f;
    [SerializeField] private LayerMask targetLayer;

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
        if (Physics.Raycast(ray, out hit, shootRange, targetLayer))
        {
            Debug.Log($"Hit: {hit.collider.gameObject.name}");

            Target target = hit.collider.GetComponent<Target>();
            if (target != null)
            {
                target.Hit();
            }

            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
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
