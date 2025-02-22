using UnityEngine;

public class Abilities : MonoBehaviour
{
    [Header("Push Ability Settings")]
    [SerializeField] private float pushPower = 10f;
    [SerializeField] private float pushRadius = 5f;
    [SerializeField] private LayerMask zombieLayer;
    [SerializeField] private WeaponHolder _weaponHolder;

    public void ActivatePush()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, pushRadius, zombieLayer);

        foreach (Collider hitCollider in hitColliders)
        {
            Rigidbody rb = hitCollider.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 pushDirection = (hitCollider.transform.position - transform.position).normalized;
                rb.AddForce(pushDirection * pushPower, ForceMode.Impulse);
            }
        }

        Debug.Log("Push Activated! " + hitColliders.Length + " zombies affected.");
    }

    public void ActivateFrenzyMode()
    {
        if (_weaponHolder != null)
        {
            if (_weaponHolder.GetIsWeaponEquipped())
            {
                //_weaponHolder.GetEquippedWeapon() as Weapon;
            }
        }
    }
}
