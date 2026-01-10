using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class Trail : MonoBehaviour
{
    // For some reason, when I just add the prefab
    // as a child manually, for example as a child
    // to burger prefab, the file preview of burger
    // in unity gets tiny and I don't like that :D
    [Required] public GameObject trail;

    private void Start()
    {
        Instantiate(trail, this.transform);
    }
}
