using System;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Multiplayer.Center.Common;
using UnityEngine;

public class PrintedForm : MonoBehaviour
{
    public Action<FormData, int> OnFormDataChanged;
    
    [ValidateInput(nameof(HasExactlySixLines), "AnswerLines must contain exactly 6 elements")]
    [SerializeField] private AnswerLine[] answerLines;
    [SerializeField] private TextMeshProUGUI formNumberText;
    
    private FormData formData;
    private int formNumber;
    
    private bool HasExactlySixLines(AnswerLine[] lines)
    {
        return lines != null && lines.Length == 6;
    }

    public void Initialize(int formNumber, FormData formData)
    {
        this.formData = formData;
        SetFormNumber(formNumber);
        for (int i = 0; i < formData.masks.Length; i++)
        {
            answerLines[i].OnCensored += OnCensored;
            answerLines[i].SetResult(formData.masks[i]); 
            answerLines[i].Censor(formData.censorships[i]); 
        }
    }

    private void SetFormNumber(int formNumber)
    {
        formNumberText.text = $"#{formNumber}";
        this.formNumber = formNumber;
    }
    
    private void OnCensored(bool censored, int lineNumber)
    {
        this.formData.censorships[lineNumber] = censored;
        OnFormDataChanged?.Invoke(formData, formNumber-1);
    }
}
