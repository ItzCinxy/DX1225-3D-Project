using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private LayerMask pickupLayer;
    private WeaponHolder weaponHolder;

    private void Start()
    {
        weaponHolder = FindObjectOfType<WeaponHolder>(); // Find the player’s weapon holder
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) // Pick up or drop weapon
        {
            TryPickupWeapon();
        }
    }

    private void TryPickupWeapon()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 3f, pickupLayer))
        {
            if (hit.collider.TryGetComponent(out Weapon weapon))
            {
                weaponHolder.EquipWeapon(weapon);
                Debug.Log("Picked up weapon: " + weapon.name);
            }
        }
    }
}