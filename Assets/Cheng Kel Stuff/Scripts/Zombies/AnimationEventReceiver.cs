using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    private MonoBehaviour zombieController;

    void Start()
    {
        // Assign separately to avoid type mismatch error
        zombieController = GetComponentInParent<StandardZombieAIController>();
        if (zombieController == null) zombieController = GetComponentInParent<ChargerAIController>();
        if (zombieController == null) zombieController = GetComponentInParent<TankZombieAIController>();
        if (zombieController == null) zombieController = GetComponentInParent<BomberZombieAIController>();
        if (zombieController == null) zombieController = GetComponentInParent<SpitterZombieAIController>();

        if (zombieController == null)
        {
            Debug.LogError("No compatible zombie AI script found on parent!");
        }
    }

    public void AttackHitEvent()
    {
        if (zombieController == null) return;

        switch (zombieController)
        {
            case StandardZombieAIController standard:
                standard.AttackHitEvent();
                break;
            case ChargerAIController charger:
                charger.AttackHitEvent();
                break;
            case TankZombieAIController tank:
                tank.AttackHitEvent();
                break;
            case BomberZombieAIController bomber:
                bomber.AttackHitEvent();
                break;
            case SpitterZombieAIController spitter:
                spitter.AttackHitEvent();
                break;
            default:
                Debug.LogError($"No attack function found for {zombieController.GetType()}!");
                break;
        }
    }
}
