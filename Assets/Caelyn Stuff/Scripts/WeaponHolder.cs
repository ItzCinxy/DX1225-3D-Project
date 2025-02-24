using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder;
    public WeaponBase equippedWeapon;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask pickupLayer;

    [Header("UI References")]
    [SerializeField] private TMP_Text ammoDisplay;
    [SerializeField] private Animator animator;

    public static Transform currentTarget; // Shared target for drone @ck

    private void Update()
    {
        if (_playerInput.actions["Shoot"].IsPressed() && equippedWeapon is Weapon)
        {
            equippedWeapon.Shoot();
            ProcessHitscanEffects();

        }

        if (_playerInput.actions["Shoot"].WasPressedThisFrame() && equippedWeapon is ProjectileWeapon)
        {
            equippedWeapon.Shoot();
            ProcessHitscanEffects();

            AlertNearbyZombies(transform.position, 6.5f);
        }

        if (_playerInput.actions["Reload"].IsPressed()) Reload();
        if (_playerInput.actions["Interact"].WasPressedThisFrame()) Interact();
        if (_playerInput.actions["Drop"].WasPressedThisFrame()) DropWeapon();
    }

    private void ProcessHitscanEffects()
    {
        // ✅ Hitscan effects (Drone Targeting & Damage)
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100f))
        {
            if (hit.collider.CompareTag("Zombie"))
            {
                currentTarget = hit.transform; // Assign hit zombie as drone's target
                hit.collider.SendMessage("TakeDamage", 10, SendMessageOptions.DontRequireReceiver);
            }

            AlertNearbyZombies(transform.position, 2.5f);
        }
    }

    private void Reload()
    {
        equippedWeapon?.Reload();
    }

    private void Interact()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, pickupRange, pickupLayer))
        {
            if (hit.collider.TryGetComponent(out LuckyBox luckyBox))
            {
                luckyBox.InteractWithBox();
                return;
            }

            if (hit.collider.TryGetComponent(out WeaponBase weapon))
            {
                EquipWeapon(weapon);
            }
        }
    }

    public void EquipWeapon(WeaponBase newWeapon)
    {
        if (equippedWeapon != null) DropWeapon();

        equippedWeapon = newWeapon;

        if (equippedWeapon.TryGetComponent(out FloatingWeapon floatingEffect))
            Destroy(floatingEffect);

        if (FindObjectOfType<LuckyBox>() is LuckyBox luckyBox)
            luckyBox.WeaponPickedUp(equippedWeapon);

        equippedWeapon.transform.SetParent(weaponHolder);
        equippedWeapon.transform.localPosition = Vector3.zero;
        equippedWeapon.transform.localRotation = Quaternion.identity;

        Rigidbody rb = equippedWeapon.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
        else equippedWeapon.gameObject.AddComponent<Rigidbody>();

        BoxCollider bc = equippedWeapon.GetComponent<BoxCollider>();
        if (bc) bc.enabled = false;
        else equippedWeapon.gameObject.AddComponent<BoxCollider>();

        animator.SetBool("IsHoldingGun", true);

        if (ammoDisplay != null)
        {
            equippedWeapon.SetAmmoDisplay(ammoDisplay);
        }
    }

    public void DropWeapon()
    {
        if (equippedWeapon == null) return;

        Rigidbody rb = equippedWeapon.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        BoxCollider bc = equippedWeapon.GetComponent<BoxCollider>();
        if (bc) bc.enabled = true;

        equippedWeapon.transform.SetParent(null);
        ammoDisplay.text = "-- / --";
        equippedWeapon = null;
        animator.SetBool("IsHoldingGun", false);
    }

    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        // Set color
        Gizmos.color = Color.green;

        // Draw a ray from the camera forward
        Vector3 start = Camera.main.transform.position;
        Vector3 direction = Camera.main.transform.forward * pickupRange;
        Gizmos.DrawRay(start, direction);

        // Draw a small sphere at the end of the ray for better visibility
        Gizmos.DrawSphere(start + direction, 0.1f);
    }

    public bool GetIsWeaponEquipped()
    {
        if (equippedWeapon != null)
        {
            return true;
        }

        return false;
    }

    public WeaponBase GetEquippedWeapon()
    {
        return equippedWeapon;
    }

    void AlertNearbyZombies(Vector3 position, float alertRange)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, alertRange);
        foreach (Collider hitCollider in hitColliders)
        {
            // Check each type of zombie and alert them
            StandardZombieAIController standardZombie = hitCollider.GetComponent<StandardZombieAIController>();
            TankZombieAIController tankZombie = hitCollider.GetComponent<TankZombieAIController>();
            ChargerAIController chargerZombie = hitCollider.GetComponent<ChargerAIController>();
            BomberZombieAIController bomberZombie = hitCollider.GetComponent<BomberZombieAIController>();
            //ScreamerZombieAIController screamerZombie = hitCollider.GetComponent<ScreamerZombieAIController>();
            //ToxicroakZombieAIController toxicroakZombie = hitCollider.GetComponent<ToxicroakZombieAIController>();
            //SpitterZombieAIController spitterZombie = hitCollider.GetComponent<SpitterZombieAIController>();

            if (standardZombie != null && !standardZombie.isDying)
            {
                standardZombie.RotateTowardPlayer();
                standardZombie.ChangeState(StandardZombieAIController.EnemyState.Run);
            }
            else if (tankZombie != null && !tankZombie.isDying)
            {
                tankZombie.RotateTowardPlayer();
                tankZombie.ChangeState(TankZombieAIController.EnemyState.Run);
            }
            else if (chargerZombie != null && !chargerZombie.isDying)
            {
                chargerZombie.RotateTowardPlayer();
                chargerZombie.ChangeState(ChargerAIController.EnemyState.Run);
            }
            else if (bomberZombie != null && !bomberZombie.isDying)
            {
                bomberZombie.RotateTowardPlayer();
                bomberZombie.ChangeState(BomberZombieAIController.EnemyState.Run);
            }
            //else if (screamerZombie != null && !screamerZombie.isDying)
            //{
            //    screamerZombie.RotateTowardPlayer();
            //    screamerZombie.ChangeState(ScreamerZombieAIController.EnemyState.Run);
            //}
            //else if (toxicroakZombie != null && !toxicroakZombie.isDying)
            //{
            //    toxicroakZombie.RotateTowardPlayer();
            //    toxicroakZombie.ChangeState(ToxicroakZombieAIController.EnemyState.Run);
            //}
            //else if (spitterZombie != null && !spitterZombie.isDying)
            //{
            //    spitterZombie.RotateTowardPlayer();
            //    spitterZombie.ChangeState(SpitterZombieAIController.EnemyState.Run);
            //}
        }
    }

}