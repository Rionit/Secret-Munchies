using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New Food", menuName = "Food")]
public class FoodScriptableObject : ScriptableObject
{
    public enum Types { BURGER, FRIES, COLA, HOTDOG, CUPCAKE }

    [Title("Food Settings")]
    [EnumToggleButtons]
    [HideLabel]
    public Types type;

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