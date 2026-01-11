using TMPro;
using UnityEngine;

public class OrderedFoodElement : MonoBehaviour
{
    public TextMeshProUGUI foodName;
    public TextMeshProUGUI amount;

    public void Initialize(string foodName, int amount)
    {
        this.foodName.text = foodName;
        this.amount.text = $"{amount.ToString()}x";
    }
}
