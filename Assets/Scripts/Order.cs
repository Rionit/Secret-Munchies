using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Order
{
    public int Id { get; private set; }
    public float CreationTime { get; private set; }
    public List<FoodAmount> OrderedFoods { get; private set; }

    public Order(int id)
    {
        Id = id;
        CreationTime = Time.time;
        OrderedFoods = new List<FoodAmount>();
    }

    public void AddFood(FoodScriptableObject food, int count)
    {
        if (count <= 0) return;
        OrderedFoods.Add(new FoodAmount(food, count));
    }
}