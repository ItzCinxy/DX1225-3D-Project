using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadePickUp : MonoBehaviour
{
    private WeaponHolder weaponHolder;
    void Start()
    {
        weaponHolder = FindObjectOfType<WeaponHolder>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            weaponHolder.IncreaseNadeAmount();
            Destroy(gameObject);
        }
    }
}
