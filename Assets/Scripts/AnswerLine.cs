using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerLine : MonoBehaviour
{
    [Required] public Checkbox a;
    [Required] public Checkbox b;
    [Required] public Checkbox c;
    [Required] public Checkbox d;
    public bool interactable = true;
    
    [Required] public TextMeshProUGUI lineNumberText;
    [Required] public int lineNumber;
    public float checkboxSpacing = 2.0f;
    
    private Checkbox[] checkboxes;
    private Image blackBar;
    
    private void Awake()
    {
        checkboxes = new[] { a, b, c, d };
        blackBar = GetComponent<Image>();
    }

    private void Start()
    {
        lineNumberText.gameObject.SetActive(lineNumberText.gameObject.activeSelf);
        
        foreach (var box in checkboxes)
        {
            box.toggle.interactable = interactable;
        }
    }

    public int resultMask()
    {
        var _resultMask = 0;

        // ABCD
        if (a.toggle.isOn) _resultMask |= 1 << 3; // 1000     
        if (b.toggle.isOn) _resultMask |= 1 << 2; // 0100
        if (c.toggle.isOn) _resultMask |= 1 << 1; // 0010
        if (d.toggle.isOn) _resultMask |= 1 << 0; // 0001
        
        return _resultMask;
    }

    public void SetResult(int result)
    {
        a.toggle.isOn = (result & (1 << 3)) != 0; // A
        b.toggle.isOn = (result & (1 << 2)) != 0; // B
        c.toggle.isOn = (result & (1 << 1)) != 0; // C
        d.toggle.isOn = (result & (1 << 0)) != 0; // D
    }

    public void Censor()
    {
        if (!interactable)
        {
            blackBar.color = new Color(0, 0, 0, 1);
        }
    }

    [Button]
    private void ShowLineNumber()
    {
        lineNumberText.gameObject.SetActive(true);
    }

    [Button]
    private void HideLineNumber()
    {
        lineNumberText.gameObject.SetActive(false);
    }

    [Button]
    private void SetCheckboxSpace()
    {
        if (checkboxes == null && a != null && b != null && c != null && d != null)
        {
            checkboxes = new[] { a, b, c, d };
        }

        foreach (var box in checkboxes)
        {
            box.layout.spacing = checkboxSpacing;
        }
    }

    private void OnValidate()
    {
        if (lineNumberText != null && lineNumberText.gameObject.activeSelf)
            lineNumberText.text = $"{lineNumber}." ;
    }
}
