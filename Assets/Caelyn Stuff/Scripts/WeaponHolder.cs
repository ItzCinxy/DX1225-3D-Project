using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder;
    public List<WeaponBase> equippedWeapons = new List<WeaponBase>(); // Supports up to 2 weapons
    private int currentWeaponIndex = 0;

    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask pickupLayer;

    [Header("UI References")]
    [SerializeField] private TMP_Text ammoDisplay;
    [SerializeField] private TMP_Text weaponListText;  // New text field for weapon list
    [SerializeField] private Animator animator;

    public static Transform currentTarget; // Shared target for drone

    private void Update()
    {
        // Check for mouse scroll wheel input (using Input.GetAxis, not InputControl scroll)
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollDelta) > 0.01f && equippedWeapons.Count > 1)
        {
            if (scrollDelta > 0)
                SwapWeaponNext();
            else
                SwapWeaponPrevious();
        }

        if (equippedWeapons.Count > 0)
        {
            WeaponBase currentWeapon = equippedWeapons[currentWeaponIndex];

            if (_playerInput.actions["Shoot"].IsPressed() && currentWeapon is Weapon)
            {
                currentWeapon.Shoot();
                ProcessHitscanEffects();
            }

            if (_playerInput.actions["Shoot"].WasPressedThisFrame() && currentWeapon is ProjectileWeapon)
            {
                currentWeapon.Shoot();
                ProcessHitscanEffects();
                AlertNearbyZombies(transform.position, 6.5f);
            }

            if (_playerInput.actions["Reload"].IsPressed())
                currentWeapon.Reload();
        }

        if (_playerInput.actions["Interact"].WasPressedThisFrame())
            Interact();

        if (_playerInput.actions["Drop"].WasPressedThisFrame())
            DropWeapon();
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
        // Prevent duplicate pickups
        if (equippedWeapons.Contains(newWeapon))
            return;

        // Remove floating effect if present
        if (newWeapon.TryGetComponent(out FloatingWeapon floatingEffect))
            Destroy(floatingEffect);

        if (FindObjectOfType<LuckyBox>() is LuckyBox luckyBox)
            luckyBox.WeaponPickedUp(newWeapon);

        newWeapon.transform.SetParent(weaponHolder);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        Rigidbody rb = newWeapon.GetComponent<Rigidbody>();
        if (rb)
            rb.isKinematic = true;
        else
            newWeapon.gameObject.AddComponent<Rigidbody>();

        BoxCollider bc = newWeapon.GetComponent<BoxCollider>();
        if (bc)
            bc.enabled = false;
        else
            newWeapon.gameObject.AddComponent<BoxCollider>();

        // If you have less than 2 weapons, add the new one
        if (equippedWeapons.Count < 2)
        {
            equippedWeapons.Add(newWeapon);
            // If it's the first weapon, activate it; otherwise, keep it inactive.
            if (equippedWeapons.Count == 1)
            {
                currentWeaponIndex = 0;
                newWeapon.gameObject.SetActive(true);
            }
            else
            {
                newWeapon.gameObject.SetActive(false);
            }
        }
        else
        {
            // Already holding 2 weapons: drop the current one and replace it
            DropWeapon();
            equippedWeapons.Add(newWeapon);
            currentWeaponIndex = equippedWeapons.Count - 1;
            newWeapon.gameObject.SetActive(true);
        }

        animator.SetBool("IsHoldingGun", true);

        if (ammoDisplay != null)
            newWeapon.SetAmmoDisplay(ammoDisplay);

        UpdateWeaponListUI();
    }

    public void DropWeapon()
    {
        if (equippedWeapons.Count == 0)
            return;

        WeaponBase currentWeapon = equippedWeapons[currentWeaponIndex];

        Rigidbody rb = currentWeapon.GetComponent<Rigidbody>();
        if (rb)
            rb.isKinematic = false;

        BoxCollider bc = currentWeapon.GetComponent<BoxCollider>();
        if (bc)
            bc.enabled = true;

        currentWeapon.transform.SetParent(null);
        ammoDisplay.text = "-- / --";

        // Remove the dropped weapon from the list
        equippedWeapons.RemoveAt(currentWeaponIndex);

        // Activate the remaining weapon, if any
        if (equippedWeapons.Count > 0)
        {
            currentWeaponIndex = 0;
            equippedWeapons[currentWeaponIndex].gameObject.SetActive(true);
        }
        else
        {
            animator.SetBool("IsHoldingGun", false);
        }

        UpdateWeaponListUI();
    }

    private void SwapWeaponNext()
    {
        equippedWeapons[currentWeaponIndex].gameObject.SetActive(false);
        currentWeaponIndex = (currentWeaponIndex + 1) % equippedWeapons.Count;
        equippedWeapons[currentWeaponIndex].gameObject.SetActive(true);
        UpdateUI();
        UpdateWeaponListUI();
    }

    private void SwapWeaponPrevious()
    {
        equippedWeapons[currentWeaponIndex].gameObject.SetActive(false);
        currentWeaponIndex--;
        if (currentWeaponIndex < 0)
            currentWeaponIndex = equippedWeapons.Count - 1;
        equippedWeapons[currentWeaponIndex].gameObject.SetActive(true);
        UpdateUI();
        UpdateWeaponListUI();
    }

    private void UpdateUI()
    {
        if (equippedWeapons.Count > 0)
        {
            animator.SetBool("IsHoldingGun", true);
            equippedWeapons[currentWeaponIndex].SetAmmoDisplay(ammoDisplay);
        }
        else
        {
            animator.SetBool("IsHoldingGun", false);
            ammoDisplay.text = "-- / --";
        }
    }

    // This method updates the UI text to show your current weapons.
    private void UpdateWeaponListUI()
    {
        if (weaponListText == null)
            return;

        string text = "Weapons:\n";
        for (int i = 0; i < equippedWeapons.Count; i++)
        {
            // Mark the active weapon with a ">" symbol.
            text += (i == currentWeaponIndex ? "> " : "  ") + equippedWeapons[i].gameObject.name + "\n";
        }
        weaponListText.text = text;
    }

    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;
        Gizmos.color = Color.green;
        Vector3 start = Camera.main.transform.position;
        Vector3 direction = Camera.main.transform.forward * pickupRange;
        Gizmos.DrawRay(start, direction);
        Gizmos.DrawSphere(start + direction, 0.1f);
    }

    public bool GetIsWeaponEquipped()
    {
        return equippedWeapons.Count > 0;
    }

    public WeaponBase GetEquippedWeapon()
    {
        if (equippedWeapons.Count > 0)
            return equippedWeapons[currentWeaponIndex];
        return null;
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