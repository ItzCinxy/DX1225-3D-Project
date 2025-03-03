using System.Collections;
using TMPro;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Ammo System")]
    [SerializeField] protected int maxMagazineSize = 10;
    [SerializeField] protected int totalAmmo = 100;
    [SerializeField] protected float fireRate = 1f;
    protected int currentAmmoInMag;
    protected bool isReloading = false;

    [Header("Reload Settings")]
    [SerializeField] protected float reloadTime = 2f;

    [Header("Weapon Audio")]
    public AudioClip[] shootingSounds;
    public AudioClip ReloadingSounds;
    private int soundIndex = 0;
    [SerializeField] AudioSource weaponAudioSource;


    [Header("UI Elements")]
    protected TMP_Text ammoDisplay;

    protected virtual void Start()
    {
        currentAmmoInMag = maxMagazineSize;
        UpdateAmmoDisplay();
    }

    private void Update()
    {
        if (currentAmmoInMag <= 0)
            Reload();
    }

    public abstract void Shoot();

    public virtual void Reload()
    {
        if (isReloading || totalAmmo <= 0 || currentAmmoInMag >= maxMagazineSize) return;
        StartCoroutine(ReloadRoutine());
    }

    protected virtual IEnumerator ReloadRoutine()
    {
        PlayReloadSound();
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
    public float GetFireRate()
    {
        return fireRate;
    }

    public void SetFireRate(float newRate)
    {
        fireRate = Mathf.Max(0.1f, newRate); // Ensure fire rate is never zero
    }

    public float GetReloadTime()
    {
        return reloadTime;
    }

    public void SetReloadTime(float newTime)
    {
        reloadTime = Mathf.Max(0.1f, newTime); // Ensure reload time is never zero
    }

    public void PlayShootSound()
    {
        if (shootingSounds.Length > 0 && weaponAudioSource != null)
        {
            weaponAudioSource.PlayOneShot(shootingSounds[soundIndex]);
            soundIndex = (soundIndex + 1) % shootingSounds.Length;
        }
    }

    public void PlayReloadSound()
    {
        if (weaponAudioSource != null)
        {
            weaponAudioSource.PlayOneShot(ReloadingSounds);
        }
    }
}