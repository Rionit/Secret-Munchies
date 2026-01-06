using System;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public Image itemImage;
    public Image itemFrame;
    public Sprite bagSprite;
    
    private void Start()
    {
        GameManager.Instance.onItemGrabbed += OnItemGrabbed;
        GameManager.Instance.onItemDropped += OnItemDropped;
    }

    private void OnDestroy()
    {
        GameManager.Instance.onItemGrabbed -= OnItemGrabbed;
        GameManager.Instance.onItemDropped -= OnItemDropped;
    }

    private void OnItemGrabbed(GameObject obj)
    {
        Food food = obj.GetComponent<Food>();
        if (food != null)
            itemImage.sprite = food.foodData.sprite;
        else
            itemImage.sprite = bagSprite;
        
        itemFrame.gameObject.SetActive(true);
    }
    
    private void OnItemDropped(GameObject obj)
    {
        itemImage.sprite = null;
        itemFrame.gameObject.SetActive(false);
    }
}
