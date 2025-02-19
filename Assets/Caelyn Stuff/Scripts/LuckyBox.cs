using UnityEngine;
using System.Collections;
using TMPro;

public class LuckyBox : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Weapon[] weaponPool; // Array of weapons that can be obtained
    [SerializeField] private Transform weaponSpawnPoint; // Where the weapon appears
    [SerializeField] private float boxCooldown = 3f; // Cooldown time before using the box again
    [SerializeField] private float weaponLifetime = 10f; // Time before weapon disappears

    [Header("References")]
    [SerializeField] private TMP_Text interactText; // UI text to display "Press E to use"
    [SerializeField] private AudioClip openBoxSound;
    [SerializeField] private AudioClip receiveWeaponSound;

    private bool isBoxActive = false;
    private Weapon floatingWeapon = null;
    private WeaponHolder playerWeaponHolder;

    private void Start()
    {
        interactText.gameObject.SetActive(false); // Hide interaction UI initially
    }

    public void InteractWithBox()
    {
        if (!isBoxActive && floatingWeapon == null)
        {
            StartCoroutine(OpenBox());
        }
    }

    private IEnumerator OpenBox()
    {
        isBoxActive = true;

        if (interactText != null)
            interactText.text = "Rolling...";

        Debug.Log("Opening Lucky Box...");

        if (openBoxSound != null)
            AudioSource.PlayClipAtPoint(openBoxSound, transform.position);

        yield return new WaitForSeconds(2f); // Simulate rolling effect

        // ✅ Make sure weaponPool is not empty
        if (weaponPool == null || weaponPool.Length == 0)
        {
            Debug.LogError("Weapon Pool is empty! Add weapon prefabs in the Inspector.");
            yield break;
        }

        // ✅ Select a random weapon prefab
        int randomIndex = Random.Range(0, weaponPool.Length);
        Weapon weaponPrefab = weaponPool[randomIndex];

        if (weaponPrefab == null)
        {
            Debug.LogError("Weapon prefab is missing in weaponPool!");
            yield break;
        }

        // ✅ Instantiate a new weapon properly
        floatingWeapon = Instantiate(weaponPrefab, weaponSpawnPoint.position, Quaternion.identity);

        // ✅ Ensure it has all components
        if (!floatingWeapon.TryGetComponent(out Collider weaponCollider))
        {
            floatingWeapon.gameObject.AddComponent<BoxCollider>(); // Add collider if missing
        }

        if (!floatingWeapon.TryGetComponent(out Rigidbody weaponRb))
        {
            weaponRb = floatingWeapon.gameObject.AddComponent<Rigidbody>(); // Add Rigidbody if missing
        }

        weaponRb.isKinematic = true; // Disable physics

        floatingWeapon.gameObject.layer = LayerMask.NameToLayer("Pickup"); // ✅ Set pickup layer

        // ✅ Add floating effect if not already attached
        if (floatingWeapon.GetComponent<FloatingWeapon>() == null)
        {
            floatingWeapon.gameObject.AddComponent<FloatingWeapon>();
        }

        interactText.text = "Press E to swap";
        Debug.Log("Weapon spawned: " + floatingWeapon.name);

        yield return new WaitForSeconds(weaponLifetime); // Wait before removing the weapon

        if (floatingWeapon != null)
        {
            Debug.Log("Weapon expired: " + floatingWeapon.name);
            Destroy(floatingWeapon.gameObject);
            floatingWeapon = null;
            interactText.text = "Press E to use";
        }

        isBoxActive = false; // Box can be used again
    }

    public void WeaponPickedUp(Weapon pickedWeapon)
    {
        if (floatingWeapon == pickedWeapon) // ✅ Only clear it if it's the same weapon
        {
            floatingWeapon = null; // ✅ Clear the reference so the box no longer tracks it
        }
    }
}
