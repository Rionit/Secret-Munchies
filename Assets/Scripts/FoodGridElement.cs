using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class FoodGridElement : MonoBehaviour
{
    [HideInInspector] public Action<FoodGridElement, int> OnValueChanged;

    [Title("Food Data")]
    [Required, Tooltip("Reference to the ScriptableObject defining this food")]
    public FoodSO food;

    [Title("UI Elements")]
    [Required, Tooltip("Text label showing current count")]
    [PreviewField(80, ObjectFieldAlignment.Center)]
    public TextMeshProUGUI counterLabel;

    [Required, Tooltip("Text label showing food name")]
    [PreviewField(80, ObjectFieldAlignment.Center)]
    public TextMeshProUGUI foodLabel;

    [Required, Tooltip("Image representing the food")]
    [PreviewField(80, ObjectFieldAlignment.Center)]
    public Image foodImage;

    [ReadOnly, ShowInInspector, Tooltip("Current count of this food")]
    public int count { get; private set; }

    private void Start()
    {
        if (food != null)
        {
            foodLabel.text = food.name;
            foodImage.sprite = food.sprite;
        }
    }

    [Button(ButtonSizes.Small), Tooltip("Increase food count")]
    public void Add()
    {
        count++;
        count = Mathf.Min(count, 10);
        counterLabel.text = count.ToString();
        OnValueChanged?.Invoke(this, count);
        AudioManager.Instance?.PlayOneShot("click");
    }

    [Button(ButtonSizes.Small), Tooltip("Decrease food count")]
    public void Remove()
    {
        count--;
        count = Mathf.Max(count, 0);
        counterLabel.text = count.ToString();
        OnValueChanged?.Invoke(this, count);
        AudioManager.Instance?.PlayOneShot("click");
    }

    [Button(ButtonSizes.Small), Tooltip("Reset food count to zero")]
    public void Reset()
    {
        count = 0;
        counterLabel.text = count.ToString();
    }
}