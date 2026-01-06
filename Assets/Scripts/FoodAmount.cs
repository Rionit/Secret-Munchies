using System;
using UnityEngine;

[Serializable]
public class FoodAmount
{
    public FoodScriptableObject food;
    public int amount;

    public FoodAmount(FoodScriptableObject food, int amount)
    {
        this.food = food;
        this.amount = amount;
    }
}