using System.Collections.Generic;
using UnityEngine;

public class Dispenser : MonoBehaviour
{
    public Transform startPosition;
    public Transform endPosition;

    public FoodScriptableObject.Types type;
    
    public List<GameObject> foodInstances;
    
}
