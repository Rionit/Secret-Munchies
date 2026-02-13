using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FoodDispenserController : MonoBehaviour
{
    public List<Dispenser> dispensers;

    public void DispenseFood(FoodSO foodData)
    {
        foreach (Dispenser dispenser in dispensers)
        {
            if (dispenser.type == foodData.type)
            {
                var instancePosition = dispenser.startPosition.position + new Vector3(-2f, -5f);
                var foodInstance = Instantiate(foodData.prefab, instancePosition, Quaternion.identity);
                foodInstance.GetComponent<Clickable>().onClick.AddListener(RemoveFood);
                foodInstance.GetComponent<Food>().foodData = foodData;
                foodInstance.GetComponent<Food>().dispenser = dispenser;

                Sequence fullSequence = DOTween.Sequence();

                fullSequence
                    .Append(AnimateFoodIn(foodInstance, dispenser.startPosition.position))
                    .Append(AnimateFoodSlide(foodInstance, adjustedEndPosition(dispenser, foodData.spacing, dispenser.foodInstances.Count)));
                
                // Has to be last
                dispenser.foodInstances.Add(foodInstance);
            }
        }
    }

    private Vector3 adjustedEndPosition(Dispenser dispenser, float spacing, int count)
    {
        var endPosition = dispenser.endPosition.position;
        var startPosition = dispenser.startPosition.position;
        Vector3 direction = (startPosition - endPosition).normalized;

        return endPosition + direction * (spacing * count);
    }

    public void RemoveFood(GameObject foodInstance)
    {
        Food food = foodInstance.GetComponent<Food>();
        var foodInstances = food.dispenser.foodInstances;
        if (foodInstance.transform.position != food.dispenser.endPosition.position)
            return;
        
        if (!GameManager.Instance.GrabItem(foodInstance)) return;
        
        foodInstances.Remove(food.gameObject);
        
        int i = 0;
        foreach (GameObject instance in foodInstances)
        {
            Debug.Log(instance.name);
            var endPosition = adjustedEndPosition(food.dispenser, food.foodData.spacing, i);
            AnimateFoodSlide(instance, endPosition);
            i++;
        }
    }

    private Sequence AnimateFoodIn(GameObject foodInstance, Vector3 startPosition)
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(foodInstance.transform.DOMove(startPosition, 1f)
            .SetEase(Ease.OutQuint));
        seq.Join(foodInstance.transform.DORotate(new Vector3(0, 0, -30f), 0.3f));

        seq.InsertCallback(1f, () => { AudioManager.Instance.PlayOneShot("sliding"); });
        
        return seq;
    }

    private Sequence AnimateFoodSlide(GameObject foodInstance, Vector3 endPosition)
    {
        Sequence seq = DOTween.Sequence();


        seq.Append(foodInstance.transform.DOMove(endPosition, 2f)
            .SetEase(Ease.OutBounce));
        seq.Join(foodInstance.transform.DORotate(Vector3.zero, 3f));
        // seq.InsertCallback(.75f, () => { AudioManager.Instance.PlayOneShot("bounce"); }); // i dont like it
        
        return seq;
    }

}
