using System.Collections;
using TMPro;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Ammo System")]
    [SerializeField] protected int maxMagazineSize = 10;
    [SerializeField] protected int totalAmmo = 100;
    protected int currentAmmoInMag;
    protected bool isReloading = false;

    [Header("Reload Settings")]
    [SerializeField] protected float reloadTime = 2f;

    [Header("UI Elements")]
    protected TMP_Text ammoDisplay;

    protected virtual void Start()
    {
        currentAmmoInMag = maxMagazineSize;
        UpdateAmmoDisplay();
    }

    public abstract void Shoot();

    public virtual void Reload()
    {
        if (isReloading || totalAmmo <= 0) return;
        StartCoroutine(ReloadRoutine());
    }

    protected virtual IEnumerator ReloadRoutine()
    {
        isReloading = true;
        if (ammoDisplay != null) ammoDisplay.text = "Reloading...";
        yield return new WaitForSeconds(reloadTime);

        int ammoToReload = Mathf.Min(maxMagazineSize - currentAmmoInMag, totalAmmo);
        currentAmmoInMag += ammoToReload;
        totalAmmo -= ammoToReload;
        isReloading = false;
        UpdateAmmoDisplay();
    }

    protected virtual void UpdateAmmoDisplay()
    {
        if (ammoDisplay != null)
            ammoDisplay.text = $"{currentAmmoInMag} / {totalAmmo}";
    }

    public void SetAmmoDisplay(TMP_Text newDisplay)
    {
        ammoDisplay = newDisplay;
        UpdateAmmoDisplay();
    }

    public void IncreaseTotalAmmo(int ammoInc)
    {
        totalAmmo += ammoInc;
        UpdateAmmoDisplay();
    }
}