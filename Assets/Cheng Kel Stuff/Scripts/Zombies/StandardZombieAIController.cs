using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardZombieAIController : MonoBehaviour
{
    public enum EnemyState { Idle, Walk, Run, Attack }
    private EnemyState currentState;

    [Header("AI Settings")]
    [SerializeField] private float roamRadius = 5f;
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float attackRange = 2f;

    [SerializeField] private float idleTime = 3f;
    [SerializeField] private float walkTime = 5f;

    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float visionAngle = 60f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Loot Drops")]
    [SerializeField] private GameObject ammoPrefab;
    [SerializeField] private GameObject healthPrefab;

    private Transform player;
    private Vector3 velocity;
    private Vector3 targetPosition;
    private Animator animator;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        ChangeState(EnemyState.Walk);
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Idle: HandleIdleState(); break;
            case EnemyState.Walk: HandleWalkState(); break;
            case EnemyState.Run: HandleRunState(); break;
            case EnemyState.Attack: HandleAttackState(); break;
        }

        transform.position += velocity * Time.deltaTime;

        if (velocity.magnitude > 0.1f)
        {
            RotateTowardsMovementDirection();
        }
    }

    void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        switch (currentState)
        {
            case EnemyState.Idle:
                velocity = Vector3.zero;
                animator.SetTrigger("Idle");
                StartCoroutine(TransitionToWalk());
                break;

            case EnemyState.Walk:
                animator.SetTrigger("Walk");
                ChooseValidRandomTarget();
                StartCoroutine(StopWalkingAfterTime());
                break;

            case EnemyState.Run:
                animator.SetTrigger("Run");
                break;

            case EnemyState.Attack:
                velocity = Vector3.zero;
                animator.SetTrigger("Attack");
                StartCoroutine(PerformAttack());
                break;
        }
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
                if (hit.collider.gameObject.CompareTag("Player")) return true;
            }
        }
        return false;
    }

    public void OnGunshotHeard(Vector3 gunshotPosition)
    {
        if (currentState == EnemyState.Run || currentState == EnemyState.Attack) return;

        if (Vector3.Distance(transform.position, gunshotPosition) < 15f)
        {
            targetPosition = gunshotPosition;
            ChangeState(EnemyState.Run);
        }
    }

    void Die()
    {
        Instantiate(ammoPrefab, transform.position, Quaternion.identity);
        Instantiate(healthPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
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

        if (!CanSeePlayer()) ChangeState(EnemyState.Walk);
    }

    void HandleAttackState()
    {
        transform.LookAt(player);
        if (Vector3.Distance(transform.position, player.position) > attackRange)
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

    IEnumerator PerformAttack()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log($"{gameObject.name} attacked!");
        if (Vector3.Distance(transform.position, player.position) > attackRange)
            ChangeState(EnemyState.Run);
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
}
