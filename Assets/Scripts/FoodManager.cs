using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FoodManager : MonoBehaviour
{
        public static FoodManager Instance { get; private set; }

    public GameObject bagPrefab;
    public Transform[] bagSpawnPoints;

    public List<FoodScriptableObject> foods;
    public FoodDispenserController foodDispenserController;
    public List<FoodGridElement> foodGridElements;

    private int currentOrderId;

    private List<Order> orders = new List<Order>();
    private Queue<Order> waitingOrders = new Queue<Order>();

    private bool[] spawnOccupied;

    private Order currentOrder;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        spawnOccupied = new bool[bagSpawnPoints.Length];
        currentOrder = new Order(currentOrderId, new List<FoodScriptableObject>());
    }

    public void StartPreparingOrder()
    {
        AIManager.Instance.ChangeToWaitQueue(currentOrderId);

        foreach (FoodGridElement foodGridElement in foodGridElements)
        {
            currentOrder.AddFood(foodGridElement.food, foodGridElement.count);
        }

        orders.Add(currentOrder);
        StartCoroutine(PrepareFood(currentOrder));
        TrySpawnOrder(currentOrder);

        currentOrderId++;
        currentOrder = new Order(currentOrderId, new List<FoodScriptableObject>());
    }

    private void TrySpawnOrder(Order order)
    {
        int freeId = GetFirstFreeSpawnPoint();

        if (freeId == -1)
        {
            // No space -> virtual queue
            Debug.LogWarning("No free spawn points found");
            waitingOrders.Enqueue(order);
            return;
        }
        
        SpawnOrderAt(order, freeId);
    }

    private void SpawnOrderAt(Order order, int spawnId)
    {
        spawnOccupied[spawnId] = true;

        GameObject bagInstance = Instantiate(
            bagPrefab,
            bagSpawnPoints[spawnId].position,
            Quaternion.identity,
            this.gameObject.transform
        );

        Bag bag = bagInstance.GetComponent<Bag>();
        bag.Initialize(spawnId, order.Id);
    }

    private int GetFirstFreeSpawnPoint()
    {
        for (int i = 0; i < spawnOccupied.Length; i++)
        {
            if (!spawnOccupied[i])
                return i;
        }
        return -1;
    }

    public void FreeSpawnPoint(int spawnId)
    {
        spawnOccupied[spawnId] = false;

        if (waitingOrders.Count > 0)
        {
            Order nextOrder = waitingOrders.Dequeue();
            TrySpawnOrder(nextOrder);
        }
    }
    
    private IEnumerator PrepareFood(Order order) {
        foreach (FoodScriptableObject foodData in order.OrderedFoods)
        {
            yield return new WaitForSeconds(foodData.prepareTime); //wait 2 seconds
            foodDispenserController.DispenseFood(foodData);
        }

        // TODO: Show the order on the diegetic UI   
    }
}