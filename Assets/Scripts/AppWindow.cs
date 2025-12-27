using System;
using DG.Tweening;
using UnityEngine;

public class AppWindow : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log("Hi :3");
        transform.localScale = new Vector3(0, 0, 0);
        transform.DOScale(new Vector3(1, 1, 1), 0.2f);
    }

    private void OnDisable()
    {
        Debug.Log("Bye :)");
    }

    public void Close()
    {
        transform.DOScale(new Vector3(0, 0, 0), 0.2f).OnComplete(() => gameObject.SetActive(false));
    }
}
