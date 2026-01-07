using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NPC : MonoBehaviour
{
    public enum States { NULL, IDLE, PATROL, ORDER_QUEUE, WAIT_QUEUE, WAIT_PATROL, COLLECT_ORDER }

    [Title("Events")]
    [HideInInspector] public Action<int> onOrderCollected;
    [HideInInspector] public Action<NPC> onArrivedAtQueue;

    [FormerlySerializedAs("dirChangeChance")]
    [Title("Movement Settings")]
    [Tooltip("Chance (1 in X) to change patrol direction")]
    [MinValue(1)]
    public int directionChangeChance = 10;

    [Tooltip("Minimum random movement speed")]
    public float minSpeed = 0.8f;

    [Tooltip("Maximum random movement speed")]
    public float maxSpeed = 3.5f;

    [Title("NavMesh Priorities")]
    [Tooltip("Avoidance priority while patrolling")]
    public int patrolPriority = 50;

    [Tooltip("Avoidance priority while queuing")]
    public int queuePriority = 10;

    [Title("Order Data")]
    [ReadOnly, Tooltip("Assigned order id")]
    public int orderId = -1;

    [EnumToggleButtons]
    [ReadOnly]
    [Tooltip("Current NPC state")]
    public States state { get; private set; } = States.PATROL;

    [ReadOnly, Tooltip("Bag currently carried by the NPC")]
    public Bag bag;

    [Title("Wanted Foods")]
    [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    [Tooltip("Foods this NPC wants to order")]
    public List<FoodAmount> wantedFoods = new();

    [Title("Runtime (Debug)")]
    [ReadOnly, ShowInInspector]
    private NavMeshAgent agent;

    [ReadOnly, ShowInInspector]
    private int currentPoint;

    [ShowInInspector, ReadOnly, LabelText("$DirectionLabel")]
    private int direction;

    private string DirectionLabel => direction == 1 ? "Direction (Clockwise)" : "Direction (Counterclockwise)";

    [ReadOnly, ShowInInspector]
    private bool hasArrivedAtQueue;

    [ReadOnly, ShowInInspector]
    private bool isPickingBag;

    private void Start()
    {
        AIManager.Instance.RegisterNPC(this);

        agent = GetComponent<NavMeshAgent>();
        agent.updateUpAxis = false;
        agent.speed = Random.Range(minSpeed, maxSpeed);

        direction *= Random.Range(0, 2) == 0 ? -1 : 1;
        SetState(state);

        wantedFoods = FoodManager.CreateRandomWantedFoods(FoodManager.Instance.foods);
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
            sequence.Append(bag.transform.DOMove(bag.transform.position + Vector3.up * 0.2f, 0.5f));
            sequence.Append(bag.transform.DOMove(transform.position, 1.5f)
                .OnComplete(() => Destroy(bag.gameObject)));
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
            direction *= Random.Range(0, directionChangeChance) == 0 ? -1 : 1;

            currentPoint = (currentPoint + direction + AIManager.Instance.patrolPoints.Length)
                           % AIManager.Instance.patrolPoints.Length;

            agent.SetDestination(AIManager.Instance.patrolPoints[currentPoint].position);
        }
    }

    [Button(ButtonSizes.Small)]
    [Tooltip("Force NPC into a new state (debug)")]
    public void SetState(States newState)
    {
        if (state == newState) return;

        hasArrivedAtQueue = false;
        state = newState;
        UpdateAgentPriority();
    }

    private void UpdateAgentPriority()
    {
        agent.avoidancePriority =
            (state == States.PATROL || state == States.WAIT_PATROL)
                ? Random.Range(queuePriority + 1, patrolPriority)
                : queuePriority;
    }
}
