using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Clickable : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent<GameObject> onClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(gameObject);
    }
}