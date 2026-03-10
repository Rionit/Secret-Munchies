using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    public Action onNPCOrderCreated;
    public Action<bool> onOrderFinished;

    [Required] public Transform guiCanvas;
    [Required] public DialogueController dialogueController;
    
    [Title("Settings")]
    [InfoBox("Maximum number of NPCs allowed in the order queue")]
    [ShowInInspector]
    private int _orderQueueSize = 4;
    [InfoBox("Maximum number of NPCs allowed in the wait queue")]
    [ShowInInspector]
    private int _waitQueueSize = 4;

    [Title("Queue Points")]

    [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    [Tooltip("NPC patrol waypoints")]
    public Transform[] patrolPoints;

    [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    [ValidateInput(
        nameof(ValidateOrderQueuePoints),
        "Order Queue Points count must match ORDER QUEUE SIZE",
        InfoMessageType.Error)]
    [Tooltip("Exact number of positions for NPCs ordering food")]
    public Transform[] orderQueuePoints;

    [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    [ValidateInput(
        nameof(ValidateWaitQueuePoints),
        "Wait Queue Points count must match WAIT QUEUE SIZE",
        InfoMessageType.Error)]
    [Tooltip("Positions for NPCs waiting for food")]
    public Transform[] waitQueuePoints;

    [Tooltip("Where NPCs collect their finished order")]
    public Transform orderCollectionPoint;

    [Tooltip("Where NPCs go after collecting order")]
    public Transform collectionExitPoint;
    
    [Title("Runtime State")]
    [ReadOnly, ShowInInspector]
    private List<NPC> allNPCs = new();

    [ReadOnly, ShowInInspector]
    private List<NPC> orderQueue = new();

    [ReadOnly, ShowInInspector]
    private List<NPC> waitQueue = new();

    [ReadOnly, ShowInInspector] 
    private bool isDialogueEnded = false;

    private bool ValidateOrderQueuePoints(Transform[] points)
    {
        return points != null && points.Length == _orderQueueSize;
    }
    
    private bool ValidateWaitQueuePoints(Transform[] points)
    {
        return points != null && points.Length == _waitQueueSize;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(MoveToQueue());
        dialogueController.OnDialogueEnded += OnDialogueEnded;
        GameManager.Instance.onItemDropped += obj =>
        {
            Bag bag = obj.GetComponent<Bag>();
            if (bag != null)
                OrderReadyForCollection(bag);
        };
    }

    private void OnDialogueEnded(TopSecretCategory category)
    {
        isDialogueEnded = true;
    }

    private IEnumerator MoveToQueue()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            TryAssignNPCToOrderQueue();
        }
    }

    public void RegisterNPC(NPC npc)
    {
        if (!allNPCs.Contains(npc))
        {
            allNPCs.Add(npc);
            npc.onArrivedAtQueue += OnNPCArrived;
            npc.onOrderCollected += FoodManager.Instance.OnOrderFinished;
        }
    }

    private void OnNPCArrived(NPC npc)
    {
        if (orderQueue.FindIndex(n => n == npc) == 0)
        {
            FoodManager.Instance.orderMakerApp.SetNPCReady(true);
            isDialogueEnded = false;
            dialogueController.StartDialogue(npc);
        }
    }

    [Button(ButtonSizes.Medium)]
    public void TryAssignNPCToOrderQueue()
    {
        if (orderQueue.Count >= _orderQueueSize) return;

        List<NPC> candidates = allNPCs.FindAll(n =>
            n.state != NPC.States.ORDER_QUEUE &&
            n.state != NPC.States.WAIT_QUEUE);

        if (candidates.Count == 0) return;

        NPC chosen = candidates[Random.Range(0, candidates.Count)];
        AddToOrderQueue(chosen);
    }

    private void AddToOrderQueue(NPC npc)
    {
        orderQueue.Add(npc);
        npc.SetState(NPC.States.ORDER_QUEUE);
    }

    public void ChangeToWaitQueue(int orderId)
    {
        StartCoroutine(ChangeToWaitQueueRoutine(orderId));
    }

    private IEnumerator ChangeToWaitQueueRoutine(int orderId)
    {
        while (!isDialogueEnded)
        {
            yield return null; // Wait one frame
        }
        
        NPC npc = orderQueue[0];
        npc.orderId = orderId;

        orderQueue.Remove(npc);
        waitQueue.Add(npc);

        npc.SetState(NPC.States.WAIT_QUEUE);
        FoodManager.Instance.orderMakerApp.SetNPCReady(false);
        onNPCOrderCreated?.Invoke();
    }

    private void OrderReadyForCollection(Bag bag)
    {
        NPC npc = allNPCs.Find(n => n.orderId == bag.orderId);
        npc.bag = bag;
        npc.SetState(NPC.States.COLLECT_ORDER);
        waitQueue.Remove(npc);
    }

    public Vector3 GetQueueDestination(NPC npc)
    {
        Transform[] queuePoints = npc.state switch
        {
            NPC.States.ORDER_QUEUE => orderQueuePoints,
            NPC.States.WAIT_QUEUE => waitQueuePoints,
            _ => orderQueuePoints
        };

        List<NPC> queue = npc.state switch
        {
            NPC.States.ORDER_QUEUE => orderQueue,
            NPC.States.WAIT_QUEUE => waitQueue,
            _ => orderQueue
        };

        int index = queue.IndexOf(npc);
        if (index < 0 || index >= queuePoints.Length)
        {
            npc.SetState(NPC.States.WAIT_PATROL);
            return npc.transform.position;
        }

        return queuePoints[index].position;
    }
}
