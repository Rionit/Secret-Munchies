using Sirenix.OdinInspector;
using TMPro;
using Unity.Multiplayer.Center.Common;
using UnityEngine;

public class PrintedForm : MonoBehaviour
{
    [ValidateInput(nameof(HasExactlySixLines), "AnswerLines must contain exactly 6 elements")]
    [SerializeField] private AnswerLine[] answerLines;
    [SerializeField] private TextMeshProUGUI formNumberText;
    
    private bool HasExactlySixLines(AnswerLine[] lines)
    {
        return lines != null && lines.Length == 6;
    }

    public void Initialize(int formNumber, int[] masks)
    {
        SetAnswers(masks);
        SetFormNumber(formNumber);
    }

    private void SetFormNumber(int formNumber)
    {
        formNumberText.text = $"#{formNumber}";
    }
    
    private void SetAnswers(int[] masks)
    {
        for (int i = 0; i < masks.Length; i++)
        {
            answerLines[i].SetResult(masks[i]); 
        }
    }
    
}
