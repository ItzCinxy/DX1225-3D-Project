using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Weapon equippedWeapon;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private float pickupRange = 3f; 
    [SerializeField] private LayerMask pickupLayer;

    private void Update()
    {
        if (_playerInput.actions["Shoot"].WasPressedThisFrame())
        {
            Debug.Log("Shoot");
            Shoot();
        }
        if (_playerInput.actions["Reload"].IsPressed())
        {
            Debug.Log("Reload");
            Reload();
        }
        if (_playerInput.actions["Interact"].WasPressedThisFrame())
        {
            Debug.Log("Interact");
            TryPickupWeapon();
        }
        if (_playerInput.actions["Drop"].WasPressedThisFrame())
        {
            Debug.Log("Dropped");
            DropWeapon();
        }
    }

    private void Shoot()
    {
        if (equippedWeapon == null) return;
        equippedWeapon.Shoot();
    }

    private void Reload()
    {
        if (equippedWeapon == null) return;
        equippedWeapon.Reload();
    }

    private void TryPickupWeapon()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, pickupRange, pickupLayer))
        {
            Debug.Log("Raycast hit: " + hit.collider.gameObject.name);

            if (hit.collider.TryGetComponent(out Weapon weapon))
            {
                Debug.Log("Weapon Found: " + weapon.name);
                EquipWeapon(weapon);
            }
            else
            {
                Debug.Log("Hit object is not a Weapon.");
            }
        }
        else
        {
            Debug.Log("No weapon detected.");
        }
    }

    public void EquipWeapon(Weapon newWeapon)
    {
        if (equippedWeapon != null)
            DropWeapon();

        equippedWeapon = newWeapon;
        equippedWeapon.transform.SetParent(weaponHolder);
        equippedWeapon.transform.localPosition = Vector3.zero;
        equippedWeapon.transform.localRotation = Quaternion.identity;

        Rigidbody rb = equippedWeapon.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        BoxCollider bc = equippedWeapon.GetComponent<BoxCollider>();
        if (bc) bc.enabled = false;

        Debug.Log("Equipped weapon: " + equippedWeapon.name);
    }

    public void DropWeapon()
    {
        if (equippedWeapon == null) return;

        Rigidbody rb = equippedWeapon.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        BoxCollider bc = equippedWeapon.GetComponent<BoxCollider>();
        if (bc) bc.enabled = true;

        equippedWeapon.transform.SetParent(null);
        Debug.Log("Dropped weapon: " + equippedWeapon.name);

        equippedWeapon = null;
    }

    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        // Set color
        Gizmos.color = Color.green;

        // Draw a ray from the camera forward
        Vector3 start = Camera.main.transform.position;
        Vector3 direction = Camera.main.transform.forward * pickupRange;
        Gizmos.DrawRay(start, direction);

        // Draw a small sphere at the end of the ray for better visibility
        Gizmos.DrawSphere(start + direction, 0.1f);
    }
}