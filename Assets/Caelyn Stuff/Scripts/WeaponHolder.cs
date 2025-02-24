using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder;
    private List<WeaponBase> equippedWeapons = new List<WeaponBase>();
    private int currentWeaponIndex = 0;

    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask pickupLayer;

    [Header("UI References")]
    [SerializeField] private TMP_Text ammoDisplay; // ✅ ONLY this script assigns it to the weapon
    [SerializeField] private TMP_Text weapon1Display;
    [SerializeField] private TMP_Text weapon2Display;
    [SerializeField] private Animator animator;

    public static Transform currentTarget;

    private void Update()
    {
        if (_playerInput.actions["Shoot"].IsPressed() && GetCurrentWeapon() is Weapon)
        {
            GetCurrentWeapon().Shoot();
            UpdateUI();
            ProcessHitscanEffects();
        }

        if (_playerInput.actions["Shoot"].WasPressedThisFrame() && GetCurrentWeapon() is ProjectileWeapon)
        {
            GetCurrentWeapon().Shoot();
            UpdateUI();
        }

        if (_playerInput.actions["Reload"].WasPressedThisFrame()) Reload();
        if (_playerInput.actions["Interact"].WasPressedThisFrame()) Interact();
        if (_playerInput.actions["Drop"].WasPressedThisFrame()) DropWeapon(currentWeaponIndex);

        if (_playerInput.actions["NextWeapon"].WasPressedThisFrame()) NextWeapon();
        if (_playerInput.actions["PreviousWeapon"].WasPressedThisFrame()) PreviousWeapon();
    }

    private void ProcessHitscanEffects()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100f))
        {
            if (hit.collider.CompareTag("Zombie"))
            {
                currentTarget = hit.transform;
                hit.collider.SendMessage("TakeDamage", 10, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private void Reload()
    {
        GetCurrentWeapon()?.Reload();
        UpdateUI();
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
        if (equippedWeapons.Count >= 2)
        {
            DropWeapon(currentWeaponIndex);
        }

        equippedWeapons.Add(newWeapon);
        newWeapon.transform.SetParent(weaponHolder);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        if (newWeapon == null) return;

        Rigidbody rb = newWeapon.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        BoxCollider bc = newWeapon.GetComponent<BoxCollider>();
        if (bc) bc.enabled = false;

        animator.SetBool("IsHoldingGun", true);

        newWeapon.gameObject.SetActive(false); // ✅ Disable initially

        if (equippedWeapons.Count == 1)
        {
            SwitchWeapon(0);
        }

        newWeapon.SetAmmoDisplay(ammoDisplay); // ✅ Make sure the weapon UI is linked

        UpdateUI();
    }

    private void DropWeapon(int weaponIndex)
    {
        if (equippedWeapons.Count == 0) return;
        if (weaponIndex < 0 || weaponIndex >= equippedWeapons.Count) return;

        WeaponBase weaponToDrop = equippedWeapons[weaponIndex];

        Rigidbody rb = weaponToDrop.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        BoxCollider bc = weaponToDrop.GetComponent<BoxCollider>();
        if (bc) bc.enabled = true;

        weaponToDrop.transform.SetParent(null);
        equippedWeapons.RemoveAt(weaponIndex);

        if (equippedWeapons.Count > 0)
        {
            currentWeaponIndex = Mathf.Clamp(currentWeaponIndex, 0, equippedWeapons.Count - 1);
            SwitchWeapon(currentWeaponIndex);
        }
        else
        {
            currentWeaponIndex = 0;
            animator.SetBool("IsHoldingGun", false);
        }

        UpdateUI();
    }

    private void SwitchWeapon(int newIndex)
    {
        if (equippedWeapons.Count == 0) return;

        if (currentWeaponIndex >= 0 && currentWeaponIndex < equippedWeapons.Count)
        {
            equippedWeapons[currentWeaponIndex].gameObject.SetActive(false);
        }

        currentWeaponIndex = newIndex;

        if (currentWeaponIndex >= 0 && currentWeaponIndex < equippedWeapons.Count)
        {
            equippedWeapons[currentWeaponIndex].gameObject.SetActive(true);
        }

        UpdateUI();
    }

    private void NextWeapon()
    {
        if (equippedWeapons.Count <= 1) return;
        int newIndex = (currentWeaponIndex + 1) % equippedWeapons.Count;
        SwitchWeapon(newIndex);
    }

    private void PreviousWeapon()
    {
        if (equippedWeapons.Count <= 1) return;
        int newIndex = (currentWeaponIndex - 1 + equippedWeapons.Count) % equippedWeapons.Count;
        SwitchWeapon(newIndex);
    }

    private void UpdateUI()
    {
        if (equippedWeapons.Count > 0)
            ammoDisplay.text = $"{GetCurrentWeapon().GetCurrentAmmo()} / {GetCurrentWeapon().GetTotalAmmo()}";
        else if (GetCurrentWeapon() == null)
        {
            ammoDisplay.text = "-- / --";
            return;
        }

        weapon1Display.text = equippedWeapons.Count > 0 ? equippedWeapons[0].name : "Empty";
        weapon2Display.text = equippedWeapons.Count > 1 ? equippedWeapons[1].name : "Empty";
    }

    private WeaponBase GetCurrentWeapon()
    {
        return equippedWeapons.Count > 0 ? equippedWeapons[currentWeaponIndex] : null;
    }

    public bool GetIsWeaponEquipped()
    {
        return equippedWeapons.Count > 0;
    }

    public WeaponBase GetEquippedWeapon()
    {
        if (equippedWeapons.Count == 0) return null;
        return equippedWeapons[currentWeaponIndex];
    }

    public bool CanPickupWeapon()
    {
        return equippedWeapons.Count < 2;
    }

    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        Gizmos.color = Color.green;
        Vector3 start = Camera.main.transform.position;
        Vector3 direction = Camera.main.transform.forward * pickupRange;
        Gizmos.DrawRay(start, direction);
        Gizmos.DrawSphere(start + direction, 0.1f);
    }
}
