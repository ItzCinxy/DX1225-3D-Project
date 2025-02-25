using UnityEngine;

public class AreaUnlock : MonoBehaviour
{
    [Header("Area Unlock Settings")]
    public GameObject areaToUnlock;
    public int areaUnlockCost = 1000; // Cost to unlock the area

    private PlayerStats playerStats; // Reference to the PlayerStats script

    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            TryUnlockArea();
        }
    }

    public void TryUnlockArea()
    {
        if (playerStats != null)
        {
            if (playerStats.GetCoinAmount() >= areaUnlockCost)
            {
                playerStats.UseCoins(areaUnlockCost);

                ObjectiveManager.Instance.UnlockArea();

                UnlockArea();
            }
        }
    }

    private void UnlockArea()
    {
        Destroy(areaToUnlock);
    }
}