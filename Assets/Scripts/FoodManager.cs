using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FoodManager : MonoBehaviour
{
    public static FoodManager Instance { get; private set; }

    public List<FoodScriptableObject> foods;
    
    public FoodDispenserController foodDispenserController;
    
    private int nextOrderId;
    
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

    public void NewOrder()
    {   
        List<FoodScriptableObject> orderedFoods = new List<FoodScriptableObject>();
        orderedFoods.Add(foods[0]);
        orderedFoods.Add(foods[0]);
        Order newOrder = new Order(nextOrderId, orderedFoods);
        nextOrderId++;
        StartCoroutine(PrepareOrder(newOrder));
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