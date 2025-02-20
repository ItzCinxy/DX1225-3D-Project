using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = new PlayerStats();
            if (playerStats != null)
            {
                playerStats.Heal(30);
                Destroy(gameObject);
            }
        }
    }
}
