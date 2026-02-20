using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

public class FoodManager : MonoBehaviour
{
    public static FoodManager Instance { get; private set; }

    [Title("Food Database")]
    [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    [Tooltip("All available food types in the game")]
    public List<FoodSO> foods;

    [Title("Bag Spawning")]
    [Required, Tooltip("Prefab used for spawned food bags")]
    public GameObject bagPrefab;

    [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    [ValidateInput(nameof(ValidateSpawnPoints),
        "Bag Spawn Points must not be empty",
        InfoMessageType.Error)]
    [Tooltip("World positions where finished orders can spawn")]
    public Transform[] bagSpawnPoints;

    [Title("References")]
    [Required]
    public FoodDispenserController foodDispenserController;

    [Required] public OrderScreen orderScreen;
    
    [Required]
    public OrderMakerApp orderMakerApp;

    [Title("Runtime (Debug)")]
    [ReadOnly, ShowInInspector]
    private int currentOrderId;

    [ReadOnly, ShowInInspector]
    private List<Order> orders = new();

    [ReadOnly, ShowInInspector]
    private Queue<Order> waitingOrders = new();

    [ReadOnly, ShowInInspector]
    private bool[] spawnOccupied;

    [ReadOnly, ShowInInspector]
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
        orderScreen.AddOrder(currentOrder);
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
            transform
        );

        Bag bag = bagInstance.GetComponent<Bag>();
        bag.transform.rotation = bagSpawnPoints[spawnId].transform.rotation;
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

    [InfoBox("Compares prepared bag contents with NPC wanted foods")]
    public static bool Compare(List<FoodAmount> bagFoods, List<FoodAmount> wantedFoods)
    {
        if (bagFoods.Count != wantedFoods.Count)
            return false;

        Dictionary<FoodSO, int> bagCount = new();
        Dictionary<FoodSO, int> wantedCount = new();

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

    [InfoBox("Creates a randomized food order for NPCs")]
    public static List<FoodAmount> CreateRandomWantedFoods(
        List<FoodSO> availableFoods,
        int minTypes = 1,
        int maxTypes = 3,
        int minAmount = 1,
        int maxAmount = 3)
    {
        List<FoodAmount> result = new();
        int typeCount = Random.Range(minTypes, maxTypes + 1);

        List<FoodSO> pool = new(availableFoods);

        for (int i = 0; i < typeCount && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            FoodSO food = pool[index];
            pool.RemoveAt(index);

            int amount = Random.Range(minAmount, maxAmount + 1);
            result.Add(new FoodAmount(food, amount));
        }

        return result;
    }

    private bool ValidateSpawnPoints(Transform[] points)
    {
        return points != null && points.Length > 0;
    }

    [Button]
    private void DebugAddFood(FoodAmount f)
    {
        for (int i = 0; i < f.amount; i++)
        {
            foodDispenserController.DispenseFood(f.food);
        }
    }

    public void OnOrderFinished(int orderId, bool result)
    {
        foreach (Order order in orders)
        {
            if (order.Id == orderId)
            {
                orders.Remove(order);
                orderScreen.RemoveOrder(order);
                break;
            }
        }
    }
}
