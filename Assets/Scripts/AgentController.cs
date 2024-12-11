using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    public NavMeshAgent agent; // Reference to the NavMeshAgent
    public Animator animator;  // Reference to the Animator
    public float idleTime = 2f; // Time to stay idle before walking again
    public float walkRadius = 10f; // Radius for random destinations within the NavMesh

    private float idleTimer = 0f;
    private bool isIdle = false;

    private void Start()
    {
        if (!agent)
            agent = GetComponent<NavMeshAgent>();

        if (!animator)
            animator = GetComponent<Animator>();

        SetRandomDestination();
    }

    private void Update()
    {
        // Check if the agent has reached its destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!isIdle)
            {
                EnterIdleState();
            }
        }

        // Handle idle state timing
        if (isIdle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTime)
            {
                ExitIdleState();
            }
        }

        // Update animator based on movement speed
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    private void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, walkRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void EnterIdleState()
    {
        isIdle = true;
        idleTimer = 0f;
        agent.isStopped = true; // Stop the agent
    }

    private void ExitIdleState()
    {
        isIdle = false;
        agent.isStopped = false; // Resume movement
        SetRandomDestination();
    }
}
