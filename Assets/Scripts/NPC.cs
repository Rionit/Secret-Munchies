using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NPC : MonoBehaviour
{
    public enum States { NULL, IDLE, PATROL, ORDER_QUEUE, WAIT_QUEUE }

    public int dirChangeChance = 10;
    public float minSpeed = 0.8f;
    public float maxSpeed = 3.5f;
    public int patrolPriority = 50;
    public int queuePriority = 10;

    public States state { get; private set; } = States.PATROL;

    private NavMeshAgent agent;
    private int currentPoint;
    private int direction = 1;

    private void Start()
    {
        AIManager.Instance.RegisterNPC(this);

        agent = GetComponent<NavMeshAgent>();
        agent.updateUpAxis = false;
        agent.speed = Random.Range(minSpeed, maxSpeed);

        direction *= Random.Range(0, 2) == 0 ? -1 : 1;
        SetState(state);
    }

    private void Update()
    {
        switch (state)
        {
            case States.PATROL:
                HandlePatrol();
                break;

            case States.ORDER_QUEUE:
            case States.WAIT_QUEUE:
                Vector3 dest = AIManager.Instance.GetQueueDestination(this);
                if (!agent.pathPending)
                    agent.SetDestination(dest);
                break;
        }
    }

    private void HandlePatrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            direction *= Random.Range(0, dirChangeChance) == 0 ? -1 : 1;
            currentPoint = (currentPoint + direction + AIManager.Instance.patrolPoints.Length)
                           % AIManager.Instance.patrolPoints.Length;

            agent.SetDestination(AIManager.Instance.patrolPoints[currentPoint].position);
        }
    }

    public void SetState(States newState)
    {
        if (state == newState) return;

        state = newState;
        UpdateAgentPriority();

        if (state == States.IDLE)
            agent.ResetPath();
    }

    private void UpdateAgentPriority()
    {
        agent.avoidancePriority = state == States.PATROL
            ? Random.Range(queuePriority + 1, patrolPriority)
            : queuePriority;
    }
}
