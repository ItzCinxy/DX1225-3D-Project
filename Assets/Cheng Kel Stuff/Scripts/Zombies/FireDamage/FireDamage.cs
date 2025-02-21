using System.Collections;
using UnityEngine;

public class FireDamage : MonoBehaviour
{
    public int damage = 3; // Fire damage per tick
    public float tickRate = 1f; // Damage interval

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(DamagePlayer(other.GetComponent<PlayerStats>()));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines(); // Stop damage when leaving the fire
        }
    }

    IEnumerator DamagePlayer(PlayerStats player)
    {
        while (player != null)
        {
            player.TakeDamage(damage);
            Debug.Log("Player is taking fire damage: " + damage);
            yield return new WaitForSeconds(tickRate);
        }
    }
}
