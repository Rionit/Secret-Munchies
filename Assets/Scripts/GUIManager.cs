using System;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public Image itemImage;
    public Image itemFrame;
    
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

    private void OnFoodGrabbed(GameObject obj)
    {
        itemImage.sprite = obj.GetComponent<Food>().foodData.sprite;
        itemFrame.gameObject.SetActive(true);
    }
    
    private void OnFoodDropped()
    {
        itemImage.sprite = null;
        itemFrame.gameObject.SetActive(false);
    }
}
