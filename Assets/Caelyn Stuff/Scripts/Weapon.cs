using UnityEngine;
using System.Collections;
using TMPro;

public class Weapon : MonoBehaviour
{
    [Header("Ammo System")]
    [SerializeField] private int maxMagazineSize = 10;
    [SerializeField] private int totalAmmo = 100;
    private int currentAmmoInMag;

    [Header("Reload Settings")]
    [SerializeField] private float reloadTime = 2f;
    private bool isReloading = false;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text ammoDisplay;

    private WeaponRaycast weaponRaycast;

    private void Start()
    {
        currentAmmoInMag = maxMagazineSize;
        weaponRaycast = GetComponent<WeaponRaycast>();
        UpdateAmmoDisplay();
    }

    public void Shoot()
    {
        if (isReloading || currentAmmoInMag <= 0) return;
        Debug.Log("WShoot");
        currentAmmoInMag--;
        UpdateAmmoDisplay();
        weaponRaycast?.FireRaycast();
    }

    public void Reload()
    {
        if (isReloading || totalAmmo <= 0) return;
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        ammoDisplay.text = "Reloading...";
        yield return new WaitForSeconds(reloadTime);

        int ammoToReload = Mathf.Min(maxMagazineSize - currentAmmoInMag, totalAmmo);
        currentAmmoInMag += ammoToReload;
        totalAmmo -= ammoToReload;
        isReloading = false;
        UpdateAmmoDisplay();
    }

    private void UpdateAmmoDisplay()
    {
        if (ammoDisplay != null)
            ammoDisplay.text = $"Ammo: {currentAmmoInMag} / {totalAmmo}";
    }
}
