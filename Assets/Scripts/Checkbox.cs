using System;
using NUnit.Framework.Constraints;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class Checkbox : MonoBehaviour
{
    [Required] public Toggle toggle;
    [Required] public HorizontalLayoutGroup layout;
}
