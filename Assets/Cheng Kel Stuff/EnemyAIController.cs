using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAIController : MonoBehaviour
{
    public enum EnemyState { Idle, Walk, Run, Attack }
    private EnemyState currentState;

    [Header("AI Settings")]
    [SerializeField] private float roamRadius = 5f; // Radius to move in while roaming
    [SerializeField] private float chaseRange = 10f; // Distance at which enemy detects player
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float idleTime = 3f; // Time before switching from idle to walk
    [SerializeField] private float walkTime = 5f; // Time before stopping when roaming
    private Transform player;
    private NavMeshAgent agent;

    [Header("Animation")]
    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        ChangeState(EnemyState.Idle);
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
            case EnemyState.Walk:
                HandleWalkState();
                break;
            case EnemyState.Run:
                HandleRunState();
                break;
            case EnemyState.Attack:
                HandleAttackState();
                break;
        }
    }

    void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        switch (currentState)
        {
            case EnemyState.Idle:
                agent.isStopped = true;
                animator.SetTrigger("Idle");
                StartCoroutine(TransitionToWalk()); // Wait before walking again
                break;

            case EnemyState.Walk:
                agent.isStopped = false;
                animator.SetTrigger("Walk");
                MoveToRandomPoint();
                StartCoroutine(StopWalkingAfterTime()); // Stop after a few seconds
                break;

            case EnemyState.Run:
                agent.isStopped = false;
                animator.SetTrigger("Run");
                break;

            case EnemyState.Attack:
                agent.isStopped = true;
                animator.SetTrigger("Attack");
                StartCoroutine(PerformAttack());
                break;
        }
    }

    void HandleIdleState()
    {
        // Check if player is nearby, start chasing
        if (Vector3.Distance(transform.position, player.position) < chaseRange)
        {
            ChangeState(EnemyState.Run);
        }
    }

    void HandleWalkState()
    {
        // If enemy reaches the target location, stop and go idle again
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            ChangeState(EnemyState.Idle);
        }

        // If player is detected, chase
        if (Vector3.Distance(transform.position, player.position) < chaseRange)
        {
            ChangeState(EnemyState.Run);
        }
    }

    void HandleRunState()
    {
        if (player == null) return;
        agent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            ChangeState(EnemyState.Attack);
        }
    }

    void HandleAttackState()
    {
        transform.LookAt(player);

        if (Vector3.Distance(transform.position, player.position) > attackRange)
        {
            ChangeState(EnemyState.Run);
        }
    }

    IEnumerator TransitionToWalk()
    {
        yield return new WaitForSeconds(idleTime);
        ChangeState(EnemyState.Walk);
    }

    void MoveToRandomPoint()
    {
        Vector3 randomPoint = transform.position + new Vector3(Random.Range(-roamRadius, roamRadius), 0, Random.Range(-roamRadius, roamRadius));
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, roamRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
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
        {
            ChangeState(EnemyState.Run);
        }
    }
}
