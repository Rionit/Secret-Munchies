using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Clickable : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent<GameObject> onClick;
    public UnityEvent<GameObject> onPointerDown;
    public UnityEvent<GameObject> onPointerUp;

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(gameObject);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        onPointerDown?.Invoke(gameObject);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onPointerUp?.Invoke(gameObject);
    }
}