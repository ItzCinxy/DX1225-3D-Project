using System.Collections;
using UnityEngine;

public class DroneAI : MonoBehaviour
{
    [Header("Drone Settings")]
    public Transform player; // Player reference
    public float followRange = 3f; // Max distance from player
    public float attackRange = 2.5f; // Attack range
    public float speed = 5f; // Movement speed
    public float rotationSpeed = 5f;
    public float floatSpeed = 0.5f; // Speed of floating movement
    public float floatHeight = 0.5f; // Max height of floating effect

    [Header("Attack Settings")]
    public float attackSpeed = 0.5f; // Attack cooldown
    public int damage = 5; // Damage per shot

    private Transform currentTarget;
    private float lastAttackTime;
    private Vector3 randomWanderTarget;
    private float wanderTimer = 0;
    private float wanderInterval = 3f; // How often the drone changes direction

    void Start()
    {
        SetRandomWanderTarget();
    }

    void Update()
    {
        if (player == null) return;

        // If there's no target, wander around the player
        if (currentTarget == null)
        {
            WanderAroundPlayer();
        }
        else
        {
            // If there's a valid target, attack it
            AttackTarget();
        }

        // Find the closest valid target every frame
        SelectTarget();

        // Floating effect to make drone movement feel natural
        ApplyFloatingMotion();
    }

    void WanderAroundPlayer()
    {
        wanderTimer += Time.deltaTime;
        if (wanderTimer >= wanderInterval)
        {
            SetRandomWanderTarget();
            wanderTimer = 0;
        }

        Vector3 moveDirection = (randomWanderTarget - transform.position).normalized;
        transform.position += moveDirection * speed * Time.deltaTime;

        // Keep the drone within follow range of the player
        if (Vector3.Distance(transform.position, player.position) > followRange)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            transform.position += directionToPlayer * speed * Time.deltaTime;
        }

        RotateTowards(randomWanderTarget);
    }

    void SetRandomWanderTarget()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-followRange, followRange),
            0,
            Random.Range(-followRange, followRange)
        );

        randomWanderTarget = player.position + randomOffset;
    }

    void SelectTarget()
    {
        // If the target is too far, reset target
        if (currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            if (distanceToTarget > attackRange)
            {
                currentTarget = null;
            }
        }

        // Prioritize player's shooting target from WeaponHolder
        if (WeaponHolder.currentTarget != null)
        {
            currentTarget = WeaponHolder.currentTarget;
            return;
        }

        // Find the closest valid zombie
        float closestDistance = float.MaxValue;
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");

        foreach (GameObject zombie in zombies)
        {
            float distance = Vector3.Distance(transform.position, zombie.transform.position);
            if (distance < closestDistance && distance <= attackRange)
            {
                closestDistance = distance;
                currentTarget = zombie.transform;
            }
        }
    }

    void AttackTarget()
    {
        if (currentTarget == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        if (distanceToTarget > attackRange)
        {
            return; // Target is out of range
        }

        // Move toward the target for a smooth approach
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Rotate towards target
        RotateTowards(currentTarget.position);

        // Shoot projectile
        if (Time.time >= lastAttackTime + attackSpeed)
        {
            Shoot();
            lastAttackTime = Time.time;
        }
    }

    void RotateTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }

    void ApplyFloatingMotion()
    {
        float floatOffset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position += new Vector3(0, floatOffset * Time.deltaTime, 0);
    }

    void Shoot()
    {
        if (currentTarget == null) return;

        // Raycast for instant damage
        RaycastHit hit;
        if (Physics.Raycast(transform.position, (currentTarget.position - transform.position).normalized, out hit, attackRange))
        {
            if (hit.collider.CompareTag("Zombie"))
            {
                // Call the specific zombie's TakeDamage function
                hit.collider.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
        }

        Debug.Log("Drone fired a raycast bullet!");
    }
}
