using UnityEngine;

public class Weapon : WeaponBase
{
    private WeaponRaycast weaponRaycast;
    [SerializeField] private Transform flashPos;
    [SerializeField] private GameObject muzzleFlash;
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
            // Instantiate muzzle flash properly
            GameObject flashInstance = Instantiate(muzzleFlash, flashPos.position, flashPos.rotation);

            // Get the ParticleSystem component and let it play
            ParticleSystem ps = flashInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Destroy(flashInstance, ps.main.duration);
            }
            PlayShootSound();
            fireRateTimer = 0f;
        }
    }
}