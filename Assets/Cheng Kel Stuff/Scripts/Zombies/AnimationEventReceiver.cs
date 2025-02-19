using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    private MonoBehaviour zombieController;

    void Start()
    {
        // Assign separately to avoid type mismatch error
        zombieController = GetComponentInParent<StandardZombieAIController>();
        if (zombieController == null) zombieController = GetComponentInParent<TankZombieAIController>();
        if (zombieController == null) zombieController = GetComponentInParent<BomberZombieAIController>();
        if (zombieController == null) zombieController = GetComponentInParent<ScreamerZombieAIController>();

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
            case TankZombieAIController tank:
                //tank.TankJumpAttack();
                break;
            case BomberZombieAIController bomber:
                //bomber.BomberExplodeAttack();
                break;
            case ScreamerZombieAIController screamer:
                //screamer.ScreamerAlert();
                break;
            default:
                Debug.LogError($"No attack function found for {zombieController.GetType()}!");
                break;
        }
    }
}
