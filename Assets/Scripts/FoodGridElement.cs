using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FoodGridElement : MonoBehaviour
{
    public FoodScriptableObject food;
    public TextMeshProUGUI counterLabel;
    public TextMeshProUGUI foodLabel;
    public Image foodImage;
    
    public int count { get; private set; }

    private void Start()
    {
        foodLabel.text = food.name;
        foodImage.sprite = food.sprite;
    }

    public void Add()
    {
        count++;
        count = Mathf.Min(count, 10);
        counterLabel.text = count.ToString();
    }

    public void Remove()
    {
        count--;
        count = Mathf.Max(count, 0);
        counterLabel.text = count.ToString();
    }
}
