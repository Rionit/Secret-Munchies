using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New Food", menuName = "Food")]
public class FoodSO : ScriptableObject
{
    public enum Types { BURGER, FRIES, COLA, HOTDOG, CUPCAKE }

    [Title("Food Settings")]
    [EnumToggleButtons]
    [HideLabel]
    public Types type;

    [Title("Text Variations")]
    [InfoBox("Possible text variations used in dialogue (burger, burgir, hamburger, etc.)")]
    [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    [InfoBox(
        "TextVariations needs to have at least one variation!",
        InfoMessageType.Error,
        VisibleIf = nameof(IsEmpty))]
    public List<string> textVariations = new();

    public string GetDisplayName(int amount = 1)
    {
        string baseName =
            textVariations.Count > 0
                ? textVariations[Random.Range(0, textVariations.Count)]
                : type.ToString().ToLower();

        if (amount > 1 && !baseName.EndsWith("s"))
            baseName += "s";

        return baseName;
    }

    private bool IsEmpty()
    {
        return textVariations.Count == 0;
    }
    
    [Title("Gameplay")]
    [BoxGroup("Gameplay")]
    [LabelText("Spacing")]
    [MinValue(0)]
    public float spacing;

    [BoxGroup("Gameplay")]
    [LabelText("Prepare Time (sec)")]
    [MinValue(0)]
    public float prepareTime;

    [Title("Visuals")]
    [HorizontalGroup("Visuals", Width = 0.5f)]
    [PreviewField(80, ObjectFieldAlignment.Center)]
    [HideLabel]
    public Sprite sprite;

    [HorizontalGroup("Visuals")]
    [PreviewField(80, ObjectFieldAlignment.Center)]
    [HideLabel]
    public GameObject prefab;
}