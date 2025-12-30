using System;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public Image itemImage;

    private void Start()
    {
        GameManager.Instance.onFoodGrabbed += OnFoodGrabbed;
        GameManager.Instance.onFoodDropped += OnFoodDropped;
    }

    private void OnDestroy()
    {
        GameManager.Instance.onFoodGrabbed -= OnFoodGrabbed;
        GameManager.Instance.onFoodDropped -= OnFoodDropped;
    }

    private void OnFoodGrabbed(FoodScriptableObject obj)
    {
        itemImage.sprite = obj.sprite;
    }
    
    private void OnFoodDropped()
    {
        itemImage.sprite = null;
    }
}
