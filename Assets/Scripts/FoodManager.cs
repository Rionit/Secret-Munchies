using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FoodManager : MonoBehaviour
{
    public static FoodManager Instance { get; private set; }

    public List<FoodScriptableObject> foods;
    
    public FoodDispenserController foodDispenserController;

    public List<FoodGridElement> foodGridElements;
    
    private int currentOrderId;
    
    private List<Order> orders = new List<Order>();
    
    private Order currentOrder;
    
    private void Awake()
    {
        // --- Singleton stuff ---
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
        currentOrder = new Order(currentOrderId, new List<FoodScriptableObject>());
    }

    public void StartPreparingOrder()
    {
        foreach (FoodGridElement foodGridElement in foodGridElements)
        {
            currentOrder.AddFood(foodGridElement.food, foodGridElement.count);
        }
        currentOrderId++;
        orders.Add(currentOrder);
        StartCoroutine(PrepareOrder(currentOrder));
        currentOrder = new Order(currentOrderId, new List<FoodScriptableObject>());
    }
    
    private IEnumerator PrepareOrder(Order order) {
        foreach (FoodScriptableObject foodData in order.OrderedFoods)
        {
            yield return new WaitForSeconds(foodData.prepareTime); //wait 2 seconds
            foodDispenserController.DispenseFood(foodData);
        }

        // TODO: Show the order on the diegetic UI   
    }
}