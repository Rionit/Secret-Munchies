using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FoodDispenserController : MonoBehaviour
{
    
    public GameObject burger;

    public List<Dispenser> dispensers;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DispenseFood(burger.GetComponent<Food>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DispenseFood(Food food)
    {
        foreach (Dispenser dispenser in dispensers)
        {
            if (dispenser.type == food.type)
            {
                Sequence mySequence = DOTween.Sequence();
                mySequence.Append(burger.transform.DOMove(dispenser.startPosition.position, 5)
                    .SetEase(Ease.OutQuint));
                mySequence.Append(burger.transform.DOMove(dispenser.endPosition.position, 5)
                    .SetEase(Ease.OutQuint));
            }
        }
    }
}
