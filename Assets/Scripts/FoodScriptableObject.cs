using System;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "New Food", menuName = "Food")]
public class FoodScriptableObject : ScriptableObject
{
    public enum Types { BURGER, FRIES, COLA, HOTDOG, CUPCAKE }

    public Types type;
    public float spacing;
    public float prepareTime;
    public GameObject prefab;
    public Sprite sprite;
}
