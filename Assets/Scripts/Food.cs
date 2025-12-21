using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Food : MonoBehaviour, IPointerClickHandler
{
    public enum Types { BURGER, FRIES, COLA }

    public Types type;

    public float spacing;
    public FoodDispenserController foodDispenserController;
    public Dispenser dispenser;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Yessiiirrr");
        foodDispenserController.RemoveFood(this);
        Destroy(gameObject);
    }
}
