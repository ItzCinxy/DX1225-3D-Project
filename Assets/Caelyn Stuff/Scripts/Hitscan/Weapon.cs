using UnityEngine;

public class Weapon : WeaponBase
{
    private WeaponRaycast weaponRaycast;

    protected override void Start()
    {
        base.Start();
        weaponRaycast = GetComponent<WeaponRaycast>();
    }

    public override void Shoot()
    {
        if (isReloading || currentAmmoInMag <= 0) return;
        currentAmmoInMag--;
        UpdateAmmoDisplay();
        weaponRaycast?.FireRaycast();
    }
}