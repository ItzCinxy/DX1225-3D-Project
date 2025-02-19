using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder;
    public Weapon equippedWeapon;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private float pickupRange = 3f; 
    [SerializeField] private LayerMask pickupLayer;

    [Header("UI References")]
    [SerializeField] private TMP_Text ammoDisplay;

    private void Update()
    {
        if (_playerInput.actions["Shoot"].WasPressedThisFrame())
        {
            Shoot();
        }
        if (_playerInput.actions["Reload"].IsPressed())
        {
            Reload();
        }
        if (_playerInput.actions["Interact"].WasPressedThisFrame())
        {
            Interact();
        }
    }

    private void Shoot()
    {
        if (equippedWeapon == null) return;

        if (equippedWeapon.TryGetComponent(out ProjectileWeapon projectileWeapon))
        {
            projectileWeapon.Shoot(); // ✅ Fire a grenade or projectile
        }
        else if (equippedWeapon.TryGetComponent(out Weapon gunWeapon))
        {
            gunWeapon.Shoot(); // ✅ Fire a hitscan gun
        }
    }


    private void Reload()
    {
        if (equippedWeapon == null) return;
        equippedWeapon.Reload();
    }

    private void Interact()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, pickupRange, pickupLayer))
        {
            Debug.Log("Raycast hit: " + hit.collider.gameObject.name);

            if (hit.collider.TryGetComponent(out LuckyBox luckyBox))
            {
                Debug.Log("Interacting with Lucky Box");
                luckyBox.InteractWithBox(); // ✅ Call Lucky Box interaction
                return; // ✅ Exit function to prevent picking up a weapon at the same time
            }

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
            Debug.Log("Nothing to interact with detected.");
        }
    }

    public void EquipWeapon(Weapon newWeapon)
    {
        if (equippedWeapon != null)
            DropWeapon();

        equippedWeapon = newWeapon;

        FloatingWeapon floatingEffect = equippedWeapon.GetComponent<FloatingWeapon>();
        if (floatingEffect != null)
            Destroy(floatingEffect);

        LuckyBox luckyBox = FindObjectOfType<LuckyBox>(); // ✅ Get the Lucky Box
        if (luckyBox != null)
        {
            luckyBox.WeaponPickedUp(equippedWeapon); // ✅ Tell it to stop tracking the weapon
        }

        equippedWeapon.transform.SetParent(weaponHolder);
        equippedWeapon.transform.localPosition = Vector3.zero;
        equippedWeapon.transform.localRotation = Quaternion.identity;

        Rigidbody rb = equippedWeapon.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
        else equippedWeapon.gameObject.AddComponent<Rigidbody>();

            BoxCollider bc = equippedWeapon.GetComponent<BoxCollider>();
        if (bc) bc.enabled = false;
        else equippedWeapon.gameObject.AddComponent<BoxCollider>();

        if (equippedWeapon.TryGetComponent(out ProjectileWeapon projectileWeapon))
        {
            Debug.Log("Equipped a Projectile Weapon: " + equippedWeapon.name);
        }
        else if (equippedWeapon.TryGetComponent(out Weapon normalWeapon))
        {
            Debug.Log("Equipped a Normal Weapon: " + equippedWeapon.name);
        }

        if (ammoDisplay != null)
        {
            equippedWeapon.SetAmmoDisplay(ammoDisplay);
        }
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