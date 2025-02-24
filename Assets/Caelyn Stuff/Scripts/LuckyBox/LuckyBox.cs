using UnityEngine;
using System.Collections;
using TMPro;

public class LuckyBox : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private WeaponBase[] weaponPool; // Array of weapons that can be obtained
    [SerializeField] private Transform weaponSpawnPoint; // Where the weapon appears
    [SerializeField] private float boxCooldown = 3f; // Cooldown time before using the box again
    [SerializeField] private float weaponLifetime = 10f; // Time before weapon disappears

    [Header("References")]
    [SerializeField] private TMP_Text interactText;
    [SerializeField] private AudioClip openBoxSound;
    [SerializeField] private AudioClip receiveWeaponSound;
    [SerializeField] private WeaponHolder playerWeaponHolder;

    private bool isBoxActive = false;
    private WeaponBase floatingWeapon = null;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private Transform playerTransform;

    [Header("Box Models")]
    [SerializeField] private GameObject openBoxModel;
    [SerializeField] private GameObject closedBoxModel;

    private void Update()
    {
        if (playerTransform == null || interactText == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        interactText.gameObject.SetActive(distanceToPlayer <= interactionRange && !isBoxActive);
    }

    private void SetBoxState(bool isOpen)
    {
        if (openBoxModel != null) openBoxModel.SetActive(isOpen);
        if (closedBoxModel != null) closedBoxModel.SetActive(!isOpen);
    }

    public void InteractWithBox()
    {
        if (!isBoxActive && floatingWeapon == null)
        {
            StartCoroutine(OpenBox());
        }
        else if (floatingWeapon != null)
        {
            TryPickupWeapon();
        }
    }

    private IEnumerator OpenBox()
    {
        isBoxActive = true;
        SetBoxState(true);

        if (interactText != null)
            interactText.text = "Rolling...";

        Debug.Log("Opening Lucky Box...");

        if (openBoxSound != null)
            AudioSource.PlayClipAtPoint(openBoxSound, transform.position);

        yield return new WaitForSeconds(2f);

        if (weaponPool == null || weaponPool.Length == 0)
        {
            Debug.LogError("Weapon Pool is empty! Add weapon prefabs in the Inspector.");
            ResetBox();
            yield break;
        }

        int randomIndex = Random.Range(0, weaponPool.Length);
        WeaponBase weaponPrefab = weaponPool[randomIndex];

        if (weaponPrefab == null)
        {
            Debug.LogError("Weapon prefab is missing in weaponPool!");
            ResetBox();
            yield break;
        }

        floatingWeapon = Instantiate(weaponPrefab, weaponSpawnPoint.position, Quaternion.identity);

        if (!floatingWeapon.TryGetComponent(out Collider weaponCollider))
            floatingWeapon.gameObject.AddComponent<BoxCollider>();

        if (!floatingWeapon.TryGetComponent(out Rigidbody weaponRb))
            weaponRb = floatingWeapon.gameObject.AddComponent<Rigidbody>();

        weaponRb.isKinematic = true;
        floatingWeapon.gameObject.layer = LayerMask.NameToLayer("Pickup");

        if (floatingWeapon.GetComponent<FloatingWeapon>() == null)
            floatingWeapon.gameObject.AddComponent<FloatingWeapon>();

        if (interactText != null)
            interactText.text = "Press 'E' to pick up";

        Debug.Log("Weapon spawned: " + floatingWeapon.name);

        yield return new WaitForSeconds(weaponLifetime);

        if (floatingWeapon != null)
        {
            if (floatingWeapon.transform.parent == null) // Only destroy if not picked up
            {
                Destroy(floatingWeapon.gameObject);
            }
            floatingWeapon = null;
        }

        ResetBox();
    }

    private void TryPickupWeapon()
    {
        if (floatingWeapon == null || playerWeaponHolder == null) return;

        WeaponBase weaponToEquip = floatingWeapon.GetComponent<Weapon>() as WeaponBase ??
                               floatingWeapon.GetComponent<ProjectileWeapon>() as WeaponBase;

        if (weaponToEquip == null)
        {
            Debug.LogError("Picked up object is not a valid weapon!");
            return;
        }

        if (playerWeaponHolder.CanPickupWeapon())
        {
            //  Find and disable the FloatingWeapon script
            FloatingWeapon floatingEffect = floatingWeapon.GetComponent<FloatingWeapon>();
            if (floatingEffect != null)
            {
                Debug.Log("Exists");
                Destroy(floatingEffect);
            }

            floatingWeapon.transform.SetParent(null);
            floatingWeapon.gameObject.SetActive(false); 

            playerWeaponHolder.EquipWeapon(floatingWeapon);

            floatingWeapon = null; 

            if (receiveWeaponSound != null)
                AudioSource.PlayClipAtPoint(receiveWeaponSound, transform.position);

            ResetBox();
        }
        else
        {
            Debug.Log("Inventory full! Drop a weapon first.");
        }
    }


    public void WeaponPickedUp(WeaponBase pickedWeapon)
    {
        if (floatingWeapon == pickedWeapon)
        {
            floatingWeapon = null;
            ResetBox();
        }
    }

    private void ResetBox()
    {
        isBoxActive = false;
        SetBoxState(false);
        if (interactText != null)
            interactText.text = "Press 'E' to use";
    }
}