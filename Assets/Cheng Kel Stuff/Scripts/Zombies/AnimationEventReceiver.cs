using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    private StandardZombieAIController zombieController;

    void Start()
    {
        // Find the main zombie script on the parent
        zombieController = GetComponentInParent<StandardZombieAIController>();
    }

    public void AttackHitEvent()
    {
        if (zombieController != null)
        {
            zombieController.AttackHitEvent(); // Redirect event to main script
        }
        else
        {
            Debug.LogError("Zombie script not found on parent!");
        }
    }
}
