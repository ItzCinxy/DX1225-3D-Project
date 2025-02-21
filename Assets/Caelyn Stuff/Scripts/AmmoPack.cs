using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPack : MonoBehaviour
{
    [SerializeField] private int incAmmoAmt;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<WeaponHolder>() != null)
        {
            // Get the PlayerStats component
            Weapon ammoStats = other.GetComponent<Weapon>();

            if (ammoStats != null)
            {
                ammoStats.IncreaseTotalAmmo(incAmmoAmt);
                Destroy(gameObject); 
            }
        }
    }
}
