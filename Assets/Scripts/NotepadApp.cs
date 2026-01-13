using System;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

public class NotepadApp : MonoBehaviour
{
    public Action<int[]> OnPrintForm;
    
    [ValidateInput(nameof(HasExactlySixLines), "AnswerLines must contain exactly 6 elements")]
    [SerializeField] private AnswerLine[] answerLines;
    
    private bool HasExactlySixLines(AnswerLine[] lines)
    {
        return lines != null && lines.Length == 6;
    }

    public void PrintForm()
    {
        OnPrintForm?.Invoke(answerLines.Select(line => line.resultMask()).ToArray());
        
        // reset values
        answerLines.ForEach(line => line.SetResult(0));
    }
}
