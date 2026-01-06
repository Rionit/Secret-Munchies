using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NPC : MonoBehaviour
{
    public enum States { NULL, IDLE, PATROL, ORDER_QUEUE, WAIT_QUEUE, WAIT_PATROL, COLLECT_ORDER }

    public Action<int> onOrderCollected;
    public Action<NPC> onArrivedAtQueue;
    
    public int dirChangeChance = 10;
    public float minSpeed = 0.8f;
    public float maxSpeed = 3.5f;
    public int patrolPriority = 50;
    public int queuePriority = 10;
    public int orderId = -1;
    
    public States state { get; private set; } = States.PATROL;
    public Bag bag;

    private NavMeshAgent agent;
    private int currentPoint;
    private int direction = 1;

    private bool hasArrivedAtQueue = false;
    private bool isPickingBag = false;

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
            case States.IDLE:
                agent.isStopped = true;
                agent.ResetPath();  
                break;
            case States.PATROL:
            case States.WAIT_PATROL:
                HandlePatrol();
                break;
            case States.ORDER_QUEUE:
            case States.WAIT_QUEUE:
                Vector3 dest = AIManager.Instance.GetQueueDestination(this);
                agent.SetDestination(dest);
                if (!hasArrivedAtQueue && agent.remainingDistance <= 0.1f && state == States.ORDER_QUEUE)
                {
                    hasArrivedAtQueue = true;
                    onArrivedAtQueue?.Invoke(this);
                    Debug.Log("Arrived at queue");
                }
                else if (agent.remainingDistance > 0.1f && state == States.ORDER_QUEUE)
                {
                    hasArrivedAtQueue = false;
                }
                break;
            case States.COLLECT_ORDER:
                agent.SetDestination(AIManager.Instance.orderCollectionPoint.position);
                HandleOrder();
                break;
        }
    }

    private void HandleOrder()
    {
        if (agent.remainingDistance <= 0.1f && !isPickingBag)
        {
            isPickingBag = true;
            GameManager.Instance.counterController.PickItem(bag.gameObject);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(bag.transform.DOMove(bag.transform.position + new Vector3(0, 0.2f, 0), .5f));
            sequence.Append(bag.transform.DOMove(transform.position, 1.5f).OnComplete(() => Destroy(bag.gameObject)));
            sequence.AppendInterval(1.0f).OnComplete(() =>
            {
                isPickingBag = false;
                bag = null;
                SetState(States.PATROL);
                onOrderCollected?.Invoke(orderId);
            });
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

        hasArrivedAtQueue = false;
        
        state = newState;
        UpdateAgentPriority();
    }

    // might have to redo this into collider trigger
    // I have found that if I have the same
    // priority for ppl leaving with finished order
    // as those in the queue
    // they will make way for each other
    // otherwise queue ppl ignore finished ppl
    // who on the other hand are stubborn and trying
    // to leave through the queue xd
    private void UpdateAgentPriority()
    {
        agent.avoidancePriority = (state == States.PATROL || state == States.WAIT_PATROL)
            ? Random.Range(queuePriority + 1, patrolPriority)
            : queuePriority;
    }
}
