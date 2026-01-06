using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    public Transform[] patrolPoints;
    public Transform[] orderQueuePoints;
    public Transform[] waitQueuePoints;
    public Transform orderCollectionPoint;
    
    private List<NPC> allNPCs = new();
    private List<NPC> orderQueue = new();
    private List<NPC> waitQueue = new();

    private const int MAX_QUEUE_SIZE = 4;

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
        GameManager.Instance.onItemDropped += obj =>
        {
            Bag bag = obj.GetComponent<Bag>();
            if (bag != null)
                OrderFinished(bag);
        };
    }

    private IEnumerator MoveToQueue()
    {
        /*
        foreach (var queuePoint in orderQueuePoints)
        {
            yield return new WaitForSeconds(3f);
            TryAssignNPCToQueue();
        }
        */
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
        }
    }

    private void OnNPCArrived(NPC npc)
    {
        Debug.LogWarning(orderQueue.FindIndex(n => n == npc));
        if (orderQueue.FindIndex(n => n == npc) == 0)
        {
            FoodManager.Instance.orderMakerApp.SetNPCReady(true);
        }
    }
    
    public void TryAssignNPCToOrderQueue()
    {
        if (orderQueue.Count >= MAX_QUEUE_SIZE) return;

        List<NPC> candidates = allNPCs.FindAll(n =>
            n.state != NPC.States.ORDER_QUEUE &&
            n.state != NPC.States.WAIT_QUEUE);

        if (candidates.Count == 0) return; // TODO: instantiate new NPC i guess

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
        NPC npc = orderQueue[0];
        npc.orderId = orderId;
        orderQueue.Remove(npc);
        waitQueue.Add(npc);
        npc.SetState(NPC.States.WAIT_QUEUE);
        FoodManager.Instance.orderMakerApp.SetNPCReady(false);
    }

    private void OrderFinished(Bag bag)
    {
        NPC npc = allNPCs.Find((npc) => npc.orderId == bag.orderId);
        npc.bag = bag;
        npc.SetState(NPC.States.COLLECT_ORDER);
        waitQueue.Remove(npc);
    }

    public Vector3 GetQueueDestination(NPC npc)
    {
        Transform[] queuePoints = npc.state switch
        {
            NPC.States.ORDER_QUEUE => orderQueuePoints, NPC.States.WAIT_QUEUE => waitQueuePoints, _ => orderQueuePoints
        };
        
        List<NPC> queue = npc.state switch
        {
            NPC.States.ORDER_QUEUE => orderQueue, NPC.States.WAIT_QUEUE => waitQueue, _ => orderQueue
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