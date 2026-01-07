using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class FoodAmount
{
    // [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.CompletelyHidden)]
    [Tooltip("The food type")]
    public FoodScriptableObject food;

    [Tooltip("How many of this food")]
    [Range(1, 10)]
    public int amount = 1;

    public FoodAmount() { }

    public FoodAmount(FoodScriptableObject food, int amount)
    {
        this.food = food;
        this.amount = amount;
    }
}