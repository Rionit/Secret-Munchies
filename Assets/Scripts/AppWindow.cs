using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public class AppWindow : MonoBehaviour
{
    public Action OnClosed;
    
    [ShowInInspector]
    private float tweenDuration = 0.2f;
    
    private void OnEnable()
    {
        Debug.Log("Hi :3");
        transform.localScale = new Vector3(0, 0, 0);
        transform.DOScale(new Vector3(1, 1, 1), tweenDuration);
    }

    private void OnDisable()
    {
        Debug.Log("Bye :)");
    }

    public void Close()
    {
        transform.DOScale(new Vector3(0, 0, 0), tweenDuration).OnComplete(() =>
        {
            gameObject.SetActive(false);
            OnClosed?.Invoke();
        });
    }
}
