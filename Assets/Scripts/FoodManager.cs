using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class FoodManager : MonoBehaviour
{
    public static FoodManager Instance { get; private set; }
    
    public List<FoodScriptableObject> foods;
    
    public GameObject bagPrefab;
    public Transform[] bagSpawnPoints;

    public FoodDispenserController foodDispenserController;
    public OrderMakerApp orderMakerApp;
    
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
        currentOrder = new Order(currentOrderId);
    }

    public void StartPreparingOrder()
    {
        AIManager.Instance.ChangeToWaitQueue(currentOrderId);

        foreach (FoodGridElement foodGridElement in orderMakerApp.foodGridElements)
        {
            currentOrder.AddFood(foodGridElement.food, foodGridElement.count);
        }

        orders.Add(currentOrder);
        StartCoroutine(PrepareFood(currentOrder));
        TrySpawnOrder(currentOrder);

        currentOrderId++;
        currentOrder = new Order(currentOrderId);
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
    
    private IEnumerator PrepareFood(Order order)
    {
        foreach (FoodAmount foodAmount in order.OrderedFoods)
        {
            for (int i = 0; i < foodAmount.amount; i++)
            {
                yield return new WaitForSeconds(foodAmount.food.prepareTime);
                foodDispenserController.DispenseFood(foodAmount.food);
            }
        }
    }
    
    public static bool Compare(List<FoodAmount> bagFoods, List<FoodAmount> wantedFoods)
    {
        if (bagFoods.Count != wantedFoods.Count)
            return false;

        Dictionary<FoodScriptableObject, int> bagCount = new();
        Dictionary<FoodScriptableObject, int> wantedCount = new();

        foreach (var f in bagFoods)
            bagCount[f.food] = f.amount;

        foreach (var f in wantedFoods)
            wantedCount[f.food] = f.amount;

        foreach (var pair in wantedCount)
        {
            if (!bagCount.TryGetValue(pair.Key, out int amount))
                return false;

            if (amount != pair.Value)
                return false;
        }

        return true;
    }
    
    public static List<FoodAmount> CreateRandomWantedFoods(
        List<FoodScriptableObject> availableFoods,
        int minTypes = 1,
        int maxTypes = 3,
        int minAmount = 1,
        int maxAmount = 3
    )
    {
        List<FoodAmount> result = new();
        int typeCount = Random.Range(minTypes, maxTypes + 1);

        List<FoodScriptableObject> pool = new List<FoodScriptableObject>(availableFoods);

        for (int i = 0; i < typeCount && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            FoodScriptableObject food = pool[index];
            pool.RemoveAt(index);

            int amount = Random.Range(minAmount, maxAmount + 1);
            result.Add(new FoodAmount(food, amount));
        }

        return result;
    }
}