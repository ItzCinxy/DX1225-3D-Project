using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Weapon equippedWeapon;
    [SerializeField] private PlayerInput _playerInput;

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
        if (_playerInput.actions["Interact"].IsPressed())
        {
            Debug.Log("WInteract");
            TryPickupWeapon();
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
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 3f))
        {
            if (hit.collider.TryGetComponent(out Weapon weapon))
            {
                EquipWeapon(weapon);
            }
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
    }

    public void DropWeapon()
    {
        if (equippedWeapon == null) return;
        equippedWeapon.transform.SetParent(null);
        equippedWeapon = null;
    }
}
