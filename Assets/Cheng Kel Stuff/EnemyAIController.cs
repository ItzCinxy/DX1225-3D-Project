using UnityEngine;
using UnityEngine.AI;

public class EnemyAIController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
    private Transform player;

    public float chaseRange = 10f;
    public float attackRange = 2f;

    private bool isWalking = false;
    private bool isAttacking = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < attackRange)
        {
            StartAttack();
        }
        else if (distance < chaseRange)
        {
            StartWalking();
        }
        else
        {
            StopWalking();
        }
    }

    void StartWalking()
    {
        if (!isWalking)
        {
            isWalking = true;
            animator.SetBool("isWalking", true);
            agent.SetDestination(player.position);
        }
    }

    void StopWalking()
    {
        if (isWalking)
        {
            isWalking = false;
            animator.SetBool("isWalking", false);
            agent.ResetPath();
        }
    }

    void StartAttack()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            isWalking = false;
            animator.SetBool("isWalking", false);
            animator.SetBool("isAttacking", true);
            agent.ResetPath();
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }
}
