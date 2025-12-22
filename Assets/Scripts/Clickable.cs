using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Clickable : MonoBehaviour, IPointerClickHandler
{
    public Action<GameObject> onClick;

    public void SetCallback(Action<GameObject> callback)
    {
        onClick = callback;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(gameObject);
    }
}