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
            yield return new WaitForSeconds(3f);
            TryAssignNPCToQueue();
        }
    }
    
    public void RegisterNPC(NPC npc)
    {
        if (!allNPCs.Contains(npc))
            allNPCs.Add(npc);
    }

    public void TryAssignNPCToQueue()
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
        Debug.Log("Adding to queue");
        orderQueue.Add(npc);
        npc.SetState(NPC.States.ORDER_QUEUE);
    }

    public void ChangeToWaitQueue()
    {
        NPC npc = orderQueue[0];
        orderQueue.Remove(npc);
        waitQueue.Add(npc);
        npc.SetState(NPC.States.WAIT_QUEUE);
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
            npc.SetState(NPC.States.PATROL);
            return npc.transform.position;
        }

        return queuePoints[index].position;
    }
}