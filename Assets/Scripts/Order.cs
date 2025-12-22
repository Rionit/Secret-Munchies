using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Order
{
    public int Id { get; private set; }
    public float CreationTime { get; private set; }
    public List<FoodScriptableObject> OrderedFoods { get; private set; }

    public Order(int id, List<FoodScriptableObject> orderedFoods)
    {
        Id = id;
        OrderedFoods = orderedFoods;
        CreationTime = Time.time;
    }
}
