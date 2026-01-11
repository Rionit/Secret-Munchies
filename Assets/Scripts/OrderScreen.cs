using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class OrderScreen : MonoBehaviour
{
    public GameObject orderPrefab;
    public GameObject orderedFoodPrefab;

    [FormerlySerializedAs("maxOrders")] public int maxVisibleOrders = 4;
    
    [ShowInInspector]
    private Dictionary<int, GameObject> visibleOrders = new Dictionary<int, GameObject>();
    [SerializeField]
    private List<Order> orders = new List<Order>();
    
    private HorizontalLayoutGroup layoutGroup;

    private void Start()
    {
        layoutGroup = GetComponent<HorizontalLayoutGroup>();
    }

    public void AddOrder(Order newOrder)
    {
        orders.Add(newOrder);
        TryShowOrders();
    }

    public void RemoveOrder(Order orderToRemove)
    {
        orders.Remove(orderToRemove);
        GameObject instance = visibleOrders[orderToRemove.Id];
        AppWindow app = visibleOrders[orderToRemove.Id].GetComponent<AppWindow>();
        app.OnClosed += () => { Destroy(instance); };
        app.Close();
        visibleOrders.Remove(orderToRemove.Id);
        TryShowOrders();
    }

    private void TryShowOrders()
    {
        if(layoutGroup != null)
            layoutGroup.childControlWidth = orders.Count > 2;
        
        if (visibleOrders.Count >= maxVisibleOrders || orders.Count == 0) return;
        
        foreach (Order order in orders)
        {
            if (!visibleOrders.ContainsKey(order.Id))
            {
                GameObject orderInstance = Instantiate(orderPrefab, transform);
                orderInstance.name = $"Order {order.Id}";
                visibleOrders.Add(order.Id, orderInstance);
                orderInstance.GetComponent<OrderElement>().Initialize(order.Id);
                foreach (FoodAmount foodAmount in order.OrderedFoods)
                {
                    GameObject foodInstance = Instantiate(orderedFoodPrefab, orderInstance.transform);
                    foodInstance.GetComponent<OrderedFoodElement>().Initialize(foodAmount.food.type.ToString(), foodAmount.amount);
                }
            }    
        }
    }
    
    [Button]
    private void TestAddOrder(FoodAmount foodAmount)
    {
        Order test = new Order(Random.Range(0, 100));
        test.OrderedFoods.Add(foodAmount);
        AddOrder(test);
    }

    [Button]
    private void TestResetOrders()
    {
        if (orders.Count == 0) return;
        foreach (Order order in orders)
        {
            DestroyImmediate(visibleOrders[order.Id]);
        }
        orders.Clear();
        visibleOrders.Clear();
    }
}
