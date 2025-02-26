using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private PlayerController _playerController;
    private bool lastFirstPersonState;
    [SerializeField] private Transform firstPersonWeaponHolder;
    [SerializeField] private Transform thirdPersonWeaponHolder;
    public List<WeaponBase> equippedWeapons = new List<WeaponBase>(); // Supports up to 2 weapons
    private int currentWeaponIndex = 0;

    [Header("Grenade")]
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private float throwForce = 10f;
    public int numOfNades = 0;

    [Header("Player Stuff")]
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask pickupLayer;

    [Header("UI References")]
    [SerializeField] private TMP_Text ammoDisplay;
    [SerializeField] private TMP_Text weaponListText;  // New text field for weapon list
    [SerializeField] private Animator animator;

    public static Transform currentTarget; // Shared target for drone

    private void Start()
    {
        lastFirstPersonState = _playerController.GetIsFirstPerson();
        UpdateWeaponListUI();
    }

    private void Update()
    {
        bool currentFirstPersonState = _playerController.GetIsFirstPerson();
        if (currentFirstPersonState != lastFirstPersonState)
        {
            UpdateWeaponHolderView();
            lastFirstPersonState = currentFirstPersonState;
        }
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

        if (_playerInput.actions["ThrowGrenade"].WasPressedThisFrame())
            ThrowNade();
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
        if (equippedWeapons.Contains(newWeapon))
            return;

        if (newWeapon.TryGetComponent(out FloatingWeapon floatingEffect))
            Destroy(floatingEffect);

        if (FindObjectOfType<LuckyBox>() is LuckyBox luckyBox)
            luckyBox.WeaponPickedUp(newWeapon);

        // Change weapon layer to "WeaponLayer" so it only renders in the Weapon Camera
        newWeapon.gameObject.layer = LayerMask.NameToLayer("Weapon");

        // Apply the layer change to all child objects (for attachments, sights, etc.)
        foreach (Transform child in newWeapon.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Weapon");
        }

        bool isFirstPerson = _playerController.GetIsFirstPerson();
        Transform activeHolder = isFirstPerson ? firstPersonWeaponHolder : thirdPersonWeaponHolder;

        newWeapon.transform.SetParent(activeHolder);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        if (newWeapon.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;
        else
            newWeapon.gameObject.AddComponent<Rigidbody>();

        if (newWeapon.TryGetComponent(out BoxCollider bc))
            bc.enabled = false;
        else
            newWeapon.gameObject.AddComponent<BoxCollider>();

        if (equippedWeapons.Count >= 2)
        {
            DropWeapon();
        }

        equippedWeapons.Add(newWeapon);
        currentWeaponIndex = equippedWeapons.Count - 1;

        ActivateCurrentWeapon();

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

        if (currentWeapon.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = false;

        if (currentWeapon.TryGetComponent(out BoxCollider bc))
            bc.enabled = true;

        // Reset the weapon layer so it can be seen in the world again
        currentWeapon.gameObject.layer = LayerMask.NameToLayer("Pickup");

        // Apply layer change to all child objects (attachments, sights, etc.)
        foreach (Transform child in currentWeapon.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Pickup");
        }

        currentWeapon.transform.SetParent(null);
        ammoDisplay.text = "-- / --";

        // Remove the dropped weapon from the list
        equippedWeapons.RemoveAt(currentWeaponIndex);

        // Activate the remaining weapon, if any
        if (equippedWeapons.Count > 0)
        {
            currentWeaponIndex = Mathf.Clamp(currentWeaponIndex, 0, equippedWeapons.Count - 1);
            ActivateCurrentWeapon();
        }
        else
        {
            animator.SetBool("IsHoldingGun", false);
        }

        UpdateWeaponListUI();
    }

    private void SwapWeaponNext()
    {
        if (equippedWeapons.Count <= 1) return;

        equippedWeapons[currentWeaponIndex].gameObject.SetActive(false);
        currentWeaponIndex = (currentWeaponIndex + 1) % equippedWeapons.Count;
        ActivateCurrentWeapon();
    }

    private void SwapWeaponPrevious()
    {
        if (equippedWeapons.Count <= 1) return;

        equippedWeapons[currentWeaponIndex].gameObject.SetActive(false);
        currentWeaponIndex = (currentWeaponIndex - 1 + equippedWeapons.Count) % equippedWeapons.Count;
        ActivateCurrentWeapon();
    }

    private void ActivateCurrentWeapon()
    {
        // ✅ Ensure only the active weapon is enabled
        for (int i = 0; i < equippedWeapons.Count; i++)
        {
            equippedWeapons[i].gameObject.SetActive(i == currentWeaponIndex);
        }

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

    private void UpdateWeaponListUI()
    {
        if (weaponListText == null)
            return;

        // Default UI when no weapons are equipped
        string weapon1 = "--";
        string weapon2 = "--";

        // Replace with actual weapon names if available
        if (equippedWeapons.Count > 0)
            weapon1 = equippedWeapons[0].gameObject.name.Replace("(Clone)", "").Trim();
        if (equippedWeapons.Count > 1)
            weapon2 = equippedWeapons[1].gameObject.name.Replace("(Clone)", "").Trim();

        // Format the list with ">" for the active weapon
        string text = (currentWeaponIndex == 0 ? "> " : "  ") + weapon1 + "\n";
        text += (currentWeaponIndex == 1 ? "> " : "  ") + weapon2;

        // Apply text to UI
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

    public void UpdateWeaponHolderView()
    {
        bool isFirstPerson = _playerController.GetIsFirstPerson();
        Transform targetHolder = isFirstPerson ? firstPersonWeaponHolder : thirdPersonWeaponHolder;

        foreach (WeaponBase weapon in equippedWeapons)
        {
            weapon.transform.SetParent(targetHolder);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;

            // Change the weapon tag based on the camera mode
            if (isFirstPerson)
            {
                weapon.gameObject.layer = LayerMask.NameToLayer("Weapon"); // Equipped weapon in FPS mode
            }
            else
            {
                weapon.gameObject.layer = LayerMask.NameToLayer("Pickup"); // Droppable weapon in TPS mode
            }

            // Apply tag change to all child objects (attachments, scopes, etc.)
            foreach (Transform child in weapon.transform)
            {
                child.gameObject.tag = weapon.gameObject.tag;
            }
        }
    }


    public void IncreaseNadeAmount()
    {
        numOfNades++;
    }

    private void ThrowNade()
    {
        if (numOfNades <= 0) return;

        // Deduct one grenade from your stash
        numOfNades--;

        // Spawn the grenade at the weaponHolder's position
        Vector3 spawnPosition = thirdPersonWeaponHolder.position;
        GameObject grenadeInstance = Instantiate(grenadePrefab, spawnPosition, Quaternion.identity);

        // Get the grenade's Rigidbody
        Rigidbody rb = grenadeInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Use the camera's forward direction for throwing, with a slight upward boost
            Transform camTransform = Camera.main.transform;
            Vector3 throwDirection = (camTransform.forward + camTransform.up * 0.5f).normalized;

            rb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
        }
        else
        {
            Debug.LogError("Grenade prefab is missing a Rigidbody component. Fix it, please.");
        }
    }
}