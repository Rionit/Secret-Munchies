
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CounterController : MonoBehaviour
{
    public GameObject[] dropPoints;

    private Dictionary<GameObject, bool> availability = new Dictionary<GameObject, bool>();

    private void Awake()
    {
        foreach (GameObject dropPoint in dropPoints)
        {
            availability[dropPoint] = false;
        }
    }

    public void DropItem()
    {
        if (GameManager.Instance.holdingItem == null) return;

        foreach (GameObject dropPoint in dropPoints)
        {
            if (!availability[dropPoint])
            {
                availability[dropPoint] = true;
                GameManager.Instance.DropItem(dropPoint.transform.position);
                GameManager.Instance.NotifyTutorial("on_bag_dropped");
                break; 
            }
        }
    }

    public void PickItem(GameObject item)
    {
        foreach (GameObject dropPoint in dropPoints)
        {
            if (dropPoint.transform.position == item.transform.position)
            {
                Debug.LogWarning("Picked item");
                availability[dropPoint] = false;
                Debug.LogWarning(availability);
                break;
            }
        }
    }
}