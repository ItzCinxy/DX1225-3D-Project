using UnityEngine;

public class Weapon : WeaponBase
{
    private WeaponRaycast weaponRaycast;
    private float fireRateTimer = 0;

    protected override void Start()
    {
        base.Start();
        weaponRaycast = GetComponent<WeaponRaycast>();
    }

    public override void Shoot()
    {
        if (isReloading || currentAmmoInMag <= 0) return;

        fireRateTimer += Time.deltaTime;

        if (fireRateTimer >= fireRate)
        {
            currentAmmoInMag--;
            UpdateAmmoDisplay();
            weaponRaycast?.FireRaycast();
            fireRateTimer = 0f;
        }
    }
}