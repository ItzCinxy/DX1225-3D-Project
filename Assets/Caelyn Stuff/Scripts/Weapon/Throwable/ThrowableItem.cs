using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowableItem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject throwableProjectilePrefab; // The object that gets thrown

    private Transform playerHand;
    
    public void Pickup(Transform hand)
    {
        playerHand = hand;

        // Attach to player's hand
        transform.SetParent(playerHand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Disable physics while held
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;
    }

    public void Throw(Transform throwPoint, float throwForce, float upwardForce)
    {
        transform.SetParent(null);

        GameObject thrownObject = Instantiate(throwableProjectilePrefab, throwPoint.position, throwPoint.rotation);
        Rigidbody rb = thrownObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(throwPoint.forward * throwForce + Vector3.up * upwardForce, ForceMode.Impulse);
        }

        Destroy(gameObject);
    }

    public void Drop()
    {
        transform.SetParent(null);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        Collider col = GetComponent<Collider>();
        if (col) col.enabled = true;
    }
}