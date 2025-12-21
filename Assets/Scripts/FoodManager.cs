using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FoodManager : MonoBehaviour
{
    public static FoodManager Instance { get; private set; }

    public FoodDispenserController foodDispenserController;
    
    private void Awake()
    {
        // --- Singleton stuff ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

    public void NewOrder()
    {
        StartCoroutine(PrepareOrder());
    }
    
    private IEnumerator PrepareOrder() {
        while(true) {
            yield return new WaitForSeconds(2); //wait 2 seconds
            break;
        }
        
        // TODO: dispense all the food from order, also create Order script
        foodDispenserController.DispenseFood(Food.Types.BURGER);
    }
}