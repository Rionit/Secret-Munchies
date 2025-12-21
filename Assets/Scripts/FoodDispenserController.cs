using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FoodDispenserController : MonoBehaviour
{
    public List<GameObject> foodPrefabs;
    public List<Dispenser> dispensers;
    

    public void DispenseFood(Food.Types type)
    {
        foreach (Dispenser dispenser in dispensers)
        {
            if (dispenser.type == type)
            {
                var prefab = foodPrefabs[(int)type];
                var instancePosition = dispenser.startPosition.position + new Vector3(-2f, -5f);
                var foodInstance = Instantiate(prefab, instancePosition, Quaternion.identity);
                Food food =  foodInstance.GetComponent<Food>();
                food.dispenser = dispenser;
                food.foodDispenserController = this;

                Sequence fullSequence = DOTween.Sequence();

                fullSequence
                    .Append(AnimateFoodIn(foodInstance, dispenser.startPosition.position))
                    .Append(AnimateFoodSlide(foodInstance, adjustedEndPosition(dispenser, food, dispenser.foodInstances.Count)));
                
                // Has to be last
                dispenser.foodInstances.Add(foodInstance);
            }
        }
    }

    private Vector3 adjustedEndPosition(Dispenser dispenser, Food food, int count)
    {
        var endPosition = dispenser.endPosition.position;
        var startPosition = dispenser.startPosition.position;
        Vector3 direction = (startPosition - endPosition).normalized;

        return endPosition + direction * (food.GetComponent<Food>().spacing * count);
    }

    public void RemoveFood(Food food)
    {
        // BUG: If i click before this finishes animating, DOTween screams at me, fix it
        var foodInstances = food.dispenser.foodInstances;
        if (foodInstances.IndexOf(food.gameObject) != 0)
            return;
        
        foodInstances.Remove(food.gameObject);
        Destroy(food.gameObject);
        
        int i = 0;
        foreach (GameObject foodInstance in foodInstances)
        {
            Debug.Log(foodInstance.name);
            var endPosition = adjustedEndPosition(food.dispenser, food, i);
            AnimateFoodSlide(foodInstance, endPosition);
            i++;
        }
    }

    private Sequence AnimateFoodIn(GameObject food, Vector3 startPosition)
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(food.transform.DOMove(startPosition, 1f)
            .SetEase(Ease.OutQuint));
        seq.Join(food.transform.DORotate(new Vector3(0, 0, -30f), 0.3f));

        return seq;
    }

    private Sequence AnimateFoodSlide(GameObject food, Vector3 endPosition)
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(food.transform.DOMove(endPosition, 2f)
            .SetEase(Ease.OutBounce));
        seq.Join(food.transform.DORotate(Vector3.zero, 3f));

        return seq;
    }

}
