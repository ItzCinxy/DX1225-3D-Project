using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BomberZombieAIController : MonoBehaviour
{
    public enum EnemyState { Idle, Walk, Run, Attack, Hit, Convulsing, Dying }
    private EnemyState currentState;

    [Header("AI Settings")]
    [SerializeField] private float roamRadius = 6f;
    [SerializeField] private float chaseRange = 12f;
    [SerializeField] private float attackRange = 0.5f;

    [SerializeField] private float idleTime = 2f;
    [SerializeField] private float walkTime = 4f;

    [SerializeField] private float walkSpeed = 1.8f;
    [SerializeField] private float runSpeed = 5f;

    [SerializeField] private float visionAngle = 70f;
    [SerializeField] private float rotationSpeed = 6f;

    [SerializeField] private LayerMask obstacleLayer;

    [Header("Health Settings")]
    private UIEnemyHealthBar healthBar;
    [SerializeField] private int maxHealth = 150;
    [SerializeField] private int currentHealth;

    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 30;
    private bool canAttack = true;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private int explosionDamage = 30;

    [Header("Fire Settings")]
    [SerializeField] private GameObject firePrefab;
    //[SerializeField] private float fireRadius = 5f;
    [SerializeField] private float fireDuration = 5f;
    [SerializeField] private int fireDamage = 3;
    [SerializeField] private float fireTickRate = 1f;

    [Header("Loot Drops")]
    [SerializeField] private GameObject ammoPrefab;
    [SerializeField] private GameObject healthPrefab;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private GameObject GrenadePickUpPrefab;

    [Header("Zombie Audio")]
    public AudioClip[] ZombieIdleSounds;
    public AudioClip ZombieHurt;
    public AudioClip ZombieDie;
    public AudioClip ZombieExplodeSound;
    public AudioClip ZombieAttack;
    [SerializeField] AudioSource audioSource;

    public float minIdleTime = 3f;
    public float maxIdleTime = 6f;

    private Coroutine idleSoundCoroutine;
    private bool isPlayingImportantSound = false;

    private Transform player;
    private CharacterController playerController;
    private PlayerStats playerstats;
    private Vector3 velocity;
    private Vector3 targetPosition;
    private Animator animator;

    public bool isDying = false;
    private bool isConvulsing = false;

    int canStartAttack = 1;

    private CapsuleCollider capsuleCollider;
    private Rigidbody _rb;
    void Start()
    {
        idleSoundCoroutine = StartCoroutine(PlayRandomIdleSound());


        animator = GetComponentInChildren<Animator>();
        healthBar = GetComponentInChildren<UIEnemyHealthBar>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null)
        {
            playerController = player.GetComponent<CharacterController>();
        }

        playerstats = FindObjectOfType<PlayerStats>();

        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }

        capsuleCollider = GetComponent<CapsuleCollider>();
        _rb = GetComponent<Rigidbody>();

        ChangeState(EnemyState.Walk);
    }

    void Update()
    {
        if (isDying || isConvulsing) return;

        switch (currentState)
        {
            case EnemyState.Idle: HandleIdleState(); break;
            case EnemyState.Walk: HandleWalkState(); break;
            case EnemyState.Run: HandleRunState(); break;
            case EnemyState.Attack: HandleAttackState(); break;
            case EnemyState.Hit: break;
        }

        if (!isDying || !isConvulsing)
        {
            transform.position += velocity * Time.deltaTime;
        }

        if (velocity.magnitude > 0.1f && !isDying)
        {
            RotateTowardsMovementDirection();
        }
    }
    public void ChangeState(EnemyState newState)
    {
        if (currentState == newState || isDying) return;
        currentState = newState;

        ResetAllAnimationBools(); // Ensure only one bool is active at a time

        Debug.Log($"Zombie changed state to: {currentState}");

        switch (currentState)
        {
            case EnemyState.Idle:
                velocity = Vector3.zero;
                animator.SetBool("Idle", true);
                StartCoroutine(TransitionToWalk());
                break;

            case EnemyState.Walk:
                animator.SetBool("Walk", true);
                ChooseValidRandomTarget();
                StartCoroutine(StopWalkingAfterTime());
                break;

            case EnemyState.Run:
                animator.SetBool("Run", true);
                break;

            case EnemyState.Attack:
                velocity = Vector3.zero;
                animator.SetBool("Attack", true);
                StartCoroutine(PerformAttack());
                break;

            case EnemyState.Hit:
                animator.SetBool("Hit", true);
                StartCoroutine(RecoverFromHit());
                break;

            case EnemyState.Convulsing:
                isConvulsing = true;
                velocity = Vector3.zero;
                if (capsuleCollider != null) capsuleCollider.enabled = false;
                if (_rb != null) _rb.isKinematic = true;
                animator.SetBool("Convulsing", true);
                StartCoroutine(ConvulseBeforeDespawn());
                break;

            case EnemyState.Dying:
                isDying = true;
                if (capsuleCollider != null) capsuleCollider.enabled = false;
                if (_rb != null) _rb.isKinematic = true;
                animator.SetBool("Die", true);
                PlayDieSound();
                StartCoroutine(DieAfterAnimation());
                break;
        }
    }

    // Helper method to reset all animation bools before setting a new one
    void ResetAllAnimationBools()
    {
        animator.SetBool("Idle", false);
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
        animator.SetBool("Attack", false);
        animator.SetBool("Hit", false);
        animator.SetBool("Die", false);
    }

    void Seek(Vector3 target, float speed)
    {
        Vector3 desiredVelocity = (target - transform.position).normalized * speed;
        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, transform.forward, out RaycastHit hit, 1.5f, obstacleLayer))
        {
            Vector3 avoidDirection = Vector3.Cross(Vector3.up, hit.normal).normalized;
            desiredVelocity = avoidDirection * speed;
        }

        Vector3 steering = (desiredVelocity - velocity) * 2f;
        velocity += steering * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, speed);
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer < visionAngle / 2)
        {
            if (Physics.Raycast(transform.position + Vector3.up * 1.5f, directionToPlayer, out RaycastHit hit, chaseRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void TakeDamage(int damage)
    {
        if (isDying || isConvulsing) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage! HP: {currentHealth}");

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        StartCoroutine(RotateTowardPlayer());

        if (currentHealth <= 0)
        {
            ChangeState(EnemyState.Convulsing);
        }
        else
        {
            ChangeState(EnemyState.Hit);
        }
    }

    public void WeaponRotateTowardsPlayer()
    {
        StartCoroutine(RotateTowardPlayer());
    }

    IEnumerator RotateTowardPlayer()
    {
        if (player == null) yield break; // Stop if no player exists

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0; // Ignore vertical rotation

        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

        float elapsedTime = 0f;
        float duration = 0.5f; // Adjust this to control rotation speed

        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation; // Ensure final rotation is exact
    }


    IEnumerator DieAfterAnimation()
    {
        yield return new WaitForSeconds(5f);

        if (this == null) yield break;

        Die();
    }

    void Die()
    {
        ObjectiveManager.Instance.ZombieKilled();
        PlayExplodeSound();
        // Randomly decide whether to drop health or ammo (50% chance for each)
        int dropChance = Random.Range(0, 6);
        if (dropChance == 0 && ammoPrefab != null)
        {
            Instantiate(ammoPrefab, transform.position, Quaternion.identity);
        }
        else if (dropChance == 1 && healthPrefab != null)
        {
            Instantiate(healthPrefab, transform.position, Quaternion.identity);
        }
        else if (dropChance == 2 && GrenadePickUpPrefab != null)
        {
            Instantiate(GrenadePickUpPrefab, transform.position, Quaternion.identity);
        }

        // Spawn fire exactly at death spot
        SpawnFireOnDeathSpot();

        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }

        playerstats.IncreaseCoin(100);

        Destroy(gameObject);
    }


    void SpawnFireOnDeathSpot()
    {
        if (firePrefab == null) return;

        // Spawn fire at the exact death position
        GameObject fireInstance = Instantiate(firePrefab, transform.position, Quaternion.identity);

        // Destroy fire after the set duration
        Destroy(fireInstance, fireDuration);

        // Attach fire damage script
        FireDamage fireDamageScript = fireInstance.AddComponent<FireDamage>();
        fireDamageScript.damage = fireDamage;
        fireDamageScript.tickRate = fireTickRate;
    }

    void RotateTowardsMovementDirection()
    {
        Vector3 moveDirection = velocity.normalized;
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void HandleIdleState()
    {
        if (CanSeePlayer()) ChangeState(EnemyState.Run);
    }

    void HandleWalkState()
    {
        Seek(targetPosition, walkSpeed);
        if (Vector3.Distance(transform.position, targetPosition) < 1f)
            ChangeState(EnemyState.Idle);

        if (CanSeePlayer()) ChangeState(EnemyState.Run);
    }

    void HandleRunState()
    {
        if (player == null) return;

        Seek(player.position, runSpeed);

        if (Vector3.Distance(transform.position, player.position) <= attackRange)
            ChangeState(EnemyState.Attack);

        if (!CanSeePlayer()) ChangeState(EnemyState.Idle);
    }

    void HandleAttackState()
    {
        transform.LookAt(player);

        if (Vector3.Distance(transform.position, player.position) <= attackRange)
            return;

        ChangeState(EnemyState.Run);
    }


    void ChooseValidRandomTarget()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = transform.position + new Vector3(Random.Range(-roamRadius, roamRadius), 0, Random.Range(-roamRadius, roamRadius));

            if (!Physics.Raycast(randomPoint + Vector3.up * 1f, Vector3.down, 2f, obstacleLayer))
            {
                targetPosition = randomPoint;
                return;
            }
        }

        targetPosition = transform.position;
    }

    public void AttackHitEvent()
    {
        if (player == null || isDying) return;

        // Ensure the player is still in attack range before applying damage
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            Debug.Log("Zombie attack landed!");
            playerstats?.TakeDamage((float)attackDamage); // Apply damage
        }

        PlayAttackSound();

        //if (explosionEffectPrefab != null)
        //{
        //    Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        //}

        ChangeState(EnemyState.Convulsing);
    }

    IEnumerator PerformAttack()
    {
        if (!canAttack || isDying) yield break;
        canAttack = false;

        animator.SetBool("Attack", true);

        yield return new WaitForSeconds(0.5f);

        if (canStartAttack <= 0)
        {
            AttackHitEvent();
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && !isDying)
        {
            StartCoroutine(PerformAttack());
        }
        else if (distanceToPlayer <= chaseRange)
        {
            animator.SetBool("Attack", false);
            canAttack = true;
            ChangeState(EnemyState.Run);
        }
        else
        {
            animator.SetBool("Attack", false);
            canAttack = true;
            ChangeState(EnemyState.Idle);
        }

        canStartAttack -= 1;
    }

    IEnumerator RecoverFromHit()
    {
        yield return new WaitForSeconds(0.5f);
        ChangeState(EnemyState.Run);
    }

    IEnumerator ConvulseBeforeDespawn()
    {
        yield return new WaitForSeconds(2f);

        TriggerExplosionDamage();

        ChangeState(EnemyState.Dying);
    }

    IEnumerator TransitionToWalk()
    {
        yield return new WaitForSeconds(Random.Range(2, idleTime));
        ChangeState(EnemyState.Walk);
    }

    IEnumerator StopWalkingAfterTime()
    {
        yield return new WaitForSeconds(walkTime);
        ChangeState(EnemyState.Idle);
    }

    void TriggerExplosionDamage()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= explosionRadius)
        {
            Debug.Log($"Player hit by explosion! Taking {explosionDamage} damage.");
            playerstats?.TakeDamage((float)explosionDamage);
        }

        // Instantiate explosion effect
        if (explosionEffectPrefab != null)
        {
            GameObject explosionInstance = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(explosionInstance, explosionInstance.GetComponent<ParticleSystem>().main.duration);
        }
    }

    void OnDrawGizmos()
    {
        if (player == null) return;

        // Set the Gizmo color
        Gizmos.color = new Color(1, 0, 0, 0.3f);

        // Draw vision cone
        Vector3 forward = transform.forward * chaseRange;
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2, 0) * forward;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        Gizmos.DrawWireSphere(transform.position + forward, 0.3f);

        // Draw filled vision cone
        Gizmos.DrawMesh(CreateVisionMesh(), transform.position, transform.rotation);
    }

    // Helper function to create a vision cone shape
    Mesh CreateVisionMesh()
    {
        Mesh mesh = new Mesh();

        int segments = 20;
        int numVertices = segments + 2;
        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Lerp(-visionAngle / 2, visionAngle / 2, i / (float)segments);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            vertices[i + 1] = direction * chaseRange;

            if (i < segments)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    private IEnumerator PlayRandomIdleSound()
    {
        while (true)
        {
            if (!isPlayingImportantSound && ZombieIdleSounds.Length > 0 && audioSource != null)
            {
                AudioClip randomClip = ZombieIdleSounds[Random.Range(0, ZombieIdleSounds.Length)];
                audioSource.PlayOneShot(randomClip);
            }

            float waitTime = Random.Range(minIdleTime, maxIdleTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    public void PlayExplodeSound()
    {
        if (ZombieHurt != null)
        {
            StopIdleSoundTemporarily(ZombieExplodeSound);
        }
    }
    public void PlayDieSound()
    {
        if (ZombieHurt != null)
        {
            StopIdleSoundTemporarily(ZombieDie);
        }
    }

    public void PlayHurtSound()
    {
        if (ZombieHurt != null)
        {
            StopIdleSoundTemporarily(ZombieHurt);
        }
    }

    public void PlayAttackSound()
    {
        if (ZombieAttack != null)
        {
            StopIdleSoundTemporarily(ZombieAttack);
        }
    }

    private void StopIdleSoundTemporarily(AudioClip newClip)
    {
        if (idleSoundCoroutine != null)
        {
            StopCoroutine(idleSoundCoroutine);
        }

        isPlayingImportantSound = true;
        audioSource.Stop(); // Stop any current idle sound
        audioSource.PlayOneShot(newClip);

        StartCoroutine(ResumeIdleSoundsAfter(newClip.length));
    }

    private IEnumerator ResumeIdleSoundsAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        isPlayingImportantSound = false;
        idleSoundCoroutine = StartCoroutine(PlayRandomIdleSound());
    }
}
