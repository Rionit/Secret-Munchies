using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[Serializable]
public class Order
{
    [ShowInInspector, ReadOnly]
    public int Id { get; private set; }

    [ShowInInspector, ReadOnly]
    public float CreationTime { get; private set; }

    [OdinSerialize, ShowInInspector]
    public List<FoodAmount> OrderedFoods { get; private set; }

    public Order(int id)
    {
        Id = id;
        CreationTime = Application.isPlaying ? Time.time : 0f;
        OrderedFoods = new List<FoodAmount>();
    }

    public void AddFood(FoodSO food, int count)
    {
        if (count <= 0 || food == null) return;
        OrderedFoods.Add(new FoodAmount(food, count));
    }
}