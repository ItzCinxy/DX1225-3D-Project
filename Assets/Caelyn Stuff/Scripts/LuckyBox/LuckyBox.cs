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
    [SerializeField] private int costToOpen = 5;
    private PlayerStats playerStats;

    [Header("References")]
    [SerializeField] private TMP_Text interactText; // UI text to display "Press E to use"
    [SerializeField] private AudioClip openBoxSound;
    [SerializeField] private AudioClip receiveWeaponSound;

    private bool isBoxActive = false;
    private WeaponBase floatingWeapon = null;
    [SerializeField] private float interactionRange = 3f; // Adjust the distance for showing text
    private Transform playerTransform; // Assign the player's transform in the Inspector

    [Header("Box Models")]
    [SerializeField] private GameObject openBoxModel;  // ✅ Assign the open model in the Inspector
    [SerializeField] private GameObject closedBoxModel; // ✅ Assign the closed model in the Inspector

    private void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("No GameObject with tag 'Player' found! Set the player's tag to 'Player'.");
        }
    }

    private void Update()
    {
        if (playerTransform == null || interactText == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= interactionRange)
        {
            interactText.gameObject.SetActive(true);
            // Make the text face the camera
            if (Camera.main != null)
            {
                interactText.transform.LookAt(Camera.main.transform);
                interactText.transform.Rotate(0, 180, 0); // Flip it to face the player properly
            }
        }
        else
        {
            interactText.gameObject.SetActive(false);
        }
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
            if (playerStats.GetCoinAmount() >= costToOpen)
            {
                StartCoroutine(OpenBox());
                playerStats.UseCoins(costToOpen);
            }
            else
            {
                StartCoroutine(FailToOpenBox());
            }
        }
    }

    private IEnumerator FailToOpenBox()
    {
        if (interactText != null)
        {
            interactText.text = "Not Enough Money"; // Show message
            yield return new WaitForSeconds(1.5f); // Wait before resetting
            interactText.text = "Press 'E' with 5 Credits to open"; // Reset to default
        }
    }



    private IEnumerator OpenBox()
    {
        isBoxActive = true;
        SetBoxState(true); // ✅ Show open box

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
            SetBoxState(false); // ✅ Close box if no weapon is found
            isBoxActive = false; // ✅ Allow interaction again
            yield break;
        }

        // ✅ Select a random weapon prefab
        int randomIndex = Random.Range(0, weaponPool.Length);
        WeaponBase weaponPrefab = weaponPool[randomIndex];

        if (weaponPrefab == null)
        {
            Debug.LogError("Weapon prefab is missing in weaponPool!");
            SetBoxState(false); // ✅ Close box if something goes wrong
            isBoxActive = false; // ✅ Allow interaction again
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

        interactText.text = "Press 'E' to pick up";
        Debug.Log("Weapon spawned: " + floatingWeapon.name);

        yield return new WaitForSeconds(weaponLifetime); // Wait before removing the weapon

        if (floatingWeapon != null)
        {
            Debug.Log("Weapon expired: " + floatingWeapon.name);
            Destroy(floatingWeapon.gameObject);
            floatingWeapon = null;
        }

        yield return new WaitForSeconds(boxCooldown); // ✅ Wait before allowing interaction again

        interactText.text = "Press 'E' with 5 Credits to open";
        SetBoxState(false);
        isBoxActive = false; // ✅ Now the player can interact again
    }

    public void WeaponPickedUp(WeaponBase pickedWeapon)
    {
        if (floatingWeapon == pickedWeapon) // Only clear it if it's the same weapon
        {
            floatingWeapon = null; // Clear the reference so the box no longer tracks it
        }
    }
}
