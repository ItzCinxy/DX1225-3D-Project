using UnityEngine;

public class ObjectiveTrail : MonoBehaviour
{
    public Transform target; // The objective to guide towards
    public float speed = 3f; // Speed of movement
    public float arrivalThreshold = 1.5f; // Stop moving when close
    public float heightOffset = 1f; // Offset above ground to avoid clipping

    [SerializeField] private TrailRenderer trailRenderer;

    private void Start()
    {
        if (trailRenderer == null)
            Debug.LogWarning("ObjectiveTrail: No Trail Renderer found on this object.");

        gameObject.SetActive(false); // Hide at start
    }

    private void Update()
    {
        if (target == null) return;

        // Calculate target position with height offset
        Vector3 targetPosition = target.position + Vector3.up * heightOffset;
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Move toward the target
        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance > arrivalThreshold)
        {
            transform.position += direction * speed * Time.deltaTime;
        }
        else
        {
            DisableTrail(); // Hide when close to target
        }
    }

    // Activate trail and move toward target
    public void SetTarget(Transform newTarget, Transform playerStart)
    {
        if (newTarget == null) return;

        target = newTarget;
        gameObject.SetActive(true);

        // Reset trail position to player’s start position
        transform.position = playerStart.position + Vector3.up * heightOffset;
        trailRenderer.Clear(); // Reset trail effect
    }

    // Disable the trail when no longer needed
    public void DisableTrail()
    {
        gameObject.SetActive(false);
        target = null;
    }
}