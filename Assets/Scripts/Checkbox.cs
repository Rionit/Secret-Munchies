using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class Checkbox : MonoBehaviour
{
    [Required] public Toggle toggle;
    [Required] public HorizontalLayoutGroup layout;

    private void Start()
    {
        toggle.onValueChanged.AddListener(isOn => { if (isOn) AudioManager.Instance.PlayOneShot("checkbox"); });
    }
}
