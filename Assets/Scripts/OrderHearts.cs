using System;
using UnityEngine;

public class OrderHearts : MonoBehaviour
{
    public GameObject[] hearts;
    
    private void Start()
    {
        GameManager.Instance.onOrderHeartsChanged += OrderHeartsChanged;
    }

    private void OrderHeartsChanged(int amount)
    {
        int i = 1;
        foreach (GameObject heart in hearts)
        {
            heart.SetActive(i <= amount);
            i++;
        }
    }
}
