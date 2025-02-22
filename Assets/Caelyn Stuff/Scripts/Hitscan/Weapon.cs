using UnityEngine;

public class Weapon : WeaponBase
{
    private WeaponRaycast weaponRaycast;
    private float firingRateTimer;

    protected override void Start()
    {
        base.Start();
        weaponRaycast = GetComponent<WeaponRaycast>();
    }

    public override void Shoot()
    {
        if (isReloading || currentAmmoInMag <= 0) return;
        firingRateTimer += Time.deltaTime;
        if (firingRateTimer >= firingRate)
        {
            currentAmmoInMag--;
            UpdateAmmoDisplay();
            weaponRaycast?.FireRaycast();
            firingRateTimer = 0f;
        }
    }
}