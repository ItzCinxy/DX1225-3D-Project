using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder;
    public WeaponBase equippedWeapon;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask pickupLayer;

    [Header("UI References")]
    [SerializeField] private TMP_Text ammoDisplay;
    [SerializeField] private Animator animator;

    public static Transform currentTarget; // Shared target for drone @ck

    private void Update()
    {
        if (_playerInput.actions["Shoot"].IsPressed()) Shoot();
        if (_playerInput.actions["Reload"].IsPressed()) Reload();
        if (_playerInput.actions["Interact"].WasPressedThisFrame()) Interact();
        if (_playerInput.actions["Drop"].WasPressedThisFrame()) DropWeapon();
    }

    private void Shoot()
    {
        if (equippedWeapon == null) return;

        equippedWeapon.Shoot();

        //SoundManager.Instance.PlayWeaponShootSound();

        // Check if the weapon is a hitscan weapon
        if (equippedWeapon is Weapon)
        {
            RaycastHit hit; // for ck
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100f))
            {
                if (hit.collider.CompareTag("Zombie"))
                {
                    currentTarget = hit.transform; // Assign hit zombie as drone's target
                    hit.collider.SendMessage("TakeDamage", 10, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }

    private void Reload()
    {
        equippedWeapon?.Reload();
    }

    private void Interact()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, pickupRange, pickupLayer))
        {
            if (hit.collider.TryGetComponent(out LuckyBox luckyBox))
            {
                luckyBox.InteractWithBox();
                return;
            }

            if (hit.collider.TryGetComponent(out WeaponBase weapon))
            {
                EquipWeapon(weapon);
            }
        }
    }

    public void EquipWeapon(WeaponBase newWeapon)
    {
        if (equippedWeapon != null) DropWeapon();

        equippedWeapon = newWeapon;

        if (equippedWeapon.TryGetComponent(out FloatingWeapon floatingEffect))
            Destroy(floatingEffect);

        if (FindObjectOfType<LuckyBox>() is LuckyBox luckyBox)
            luckyBox.WeaponPickedUp(equippedWeapon);

        equippedWeapon.transform.SetParent(weaponHolder);
        equippedWeapon.transform.localPosition = Vector3.zero;
        equippedWeapon.transform.localRotation = Quaternion.identity;

        Rigidbody rb = equippedWeapon.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
        else equippedWeapon.gameObject.AddComponent<Rigidbody>();

        BoxCollider bc = equippedWeapon.GetComponent<BoxCollider>();
        if (bc) bc.enabled = false;
        else equippedWeapon.gameObject.AddComponent<BoxCollider>();

        animator.SetBool("IsHoldingGun", true);

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
        ammoDisplay.text = "-- / --";
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

    public bool GetIsWeaponEquipped()
    {
        if (equippedWeapon != null)
        {
            return true;
        }

        return false;
    }

    public WeaponBase GetEquippedWeapon()
    {
        return equippedWeapon;
    }
}