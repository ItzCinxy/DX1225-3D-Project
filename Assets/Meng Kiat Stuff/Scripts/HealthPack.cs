using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [SerializeField] private int incHealthAmt;
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object has a CharacterController (means it's a player)
        if (other.GetComponent<CharacterController>() != null)
        {
            // Get the PlayerStats component
            PlayerStats playerStats = other.GetComponent<PlayerStats>();

            if (playerStats != null)
            { 
                playerStats.Heal(incHealthAmt); // Heal the player
                Destroy(gameObject); // Remove the health pack
            }
        }
    }
}
