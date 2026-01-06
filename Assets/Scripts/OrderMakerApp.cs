using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OrderMakerApp : MonoBehaviour
{
    public Button createOrderButton;
    public List<FoodGridElement> foodGridElements;
    
    private bool isNPCReady = false;
    private bool isNonzeroOrder = false;

    private void Start()
    {
        createOrderButton.interactable = false;
        foreach (FoodGridElement element in foodGridElements)
        {
            element.OnValueChanged += OnFoodGridElementValueChanged;
        }
    }

    public void Reset()
    {
        foreach (FoodGridElement element in foodGridElements)
        {
            element.Reset();
        }
        isNonzeroOrder = false;
    }

    public void SetNPCReady(bool isNPCReady)
    {
        this.isNPCReady = isNPCReady;
        TryEnableButton();
    }

    private void OnFoodGridElementValueChanged(FoodGridElement element, int newValue)
    {
        if (newValue > 0) isNonzeroOrder = true;
        else isNonzeroOrder = foodGridElements.Any(el => el.count != 0);

        TryEnableButton();
    }

    private void TryEnableButton()
    {
        createOrderButton.interactable = isNonzeroOrder && isNPCReady;
    } 
}
