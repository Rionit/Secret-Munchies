using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class NPC : MonoBehaviour
{
    public Transform[] patrolPoints;
    
    public const int DIR_CHANGE_CHANCE = 10;
    public const float MIN_SPEED = 0.8f;
    public const float MAX_SPEED = 3.5f;
    
    private int currentPoint = 0;
    private int direction = 1;
    private NavMeshAgent agent;

    private void Start()
    {
        direction *= Random.Range(0, 2) == 0 ? -1 : 1;
        
        agent = GetComponent<NavMeshAgent>();
        agent.updateUpAxis = false;
        agent.speed = Random.Range(MIN_SPEED, MAX_SPEED);
        
        agent.SetDestination(patrolPoints[currentPoint].position);
    }

    private void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            direction *= Random.Range(0, DIR_CHANGE_CHANCE) == 0 ? -1 : 1;
            currentPoint = (Math.Max(0, currentPoint + direction)) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPoint].position);
        }
    }
}
