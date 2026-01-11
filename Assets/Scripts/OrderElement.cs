using TMPro;
using UnityEngine;

public class OrderElement : MonoBehaviour
{
    public TextMeshProUGUI orderId;

    public void Initialize(int orderId)
    {
        this.orderId.text = orderId.ToString();
    }
}
