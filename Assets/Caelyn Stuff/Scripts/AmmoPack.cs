using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPack : MonoBehaviour
{
    [SerializeField] private int increaseAmmoAmt;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the player
        if (other.CompareTag("Player"))
        {
            // Get the WeaponHolder component from the player
            WeaponHolder weaponHolder = other.GetComponent<WeaponHolder>();

            // Ensure WeaponHolder exists and has an equipped weapon
            if (weaponHolder != null && weaponHolder.GetIsWeaponEquipped())
            {
                // Get the equipped weapon (Generalized for all weapons)
                WeaponBase equippedWeapon = weaponHolder.GetEquippedWeapon();

                if (equippedWeapon != null)
                {
                    equippedWeapon.IncreaseTotalAmmo(increaseAmmoAmt);
                    Destroy(gameObject); // Destroy ammo pack after pickup
                }
            }
        }
    }
}