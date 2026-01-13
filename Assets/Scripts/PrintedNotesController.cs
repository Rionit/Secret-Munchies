using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

public class PrintedNotesController : MonoBehaviour
{
    [Required] public Transform startTransform; 
    [Required] public Transform endTransform; 
    
    public PrintedForm formA;
    public PrintedForm formB;
    
    [ShowInInspector]
    private Dictionary<int, int[]> forms = new Dictionary<int, int[]>();

    [ShowInInspector]
    private int currentForm = -1;

    [ShowInInspector]
    private PrintedForm front;
    [ShowInInspector]
    private PrintedForm back;
    
    
    private Vector3 frontPos;
    private Vector3 backPos;
    private Quaternion frontRot;
    private Quaternion backRot;

    private Sequence switchAnimation;
    private Sequence showAnimation;
    
    private void Start()
    {
        front = formA;
        back = formB;
        
        frontPos = front.transform.localPosition;
        backPos = back.transform.localPosition;
        frontRot = front.transform.localRotation;
        backRot = back.transform.localRotation;

        HidePrintedNotes();
        GameManager.Instance.notepadApp.OnPrintForm += PrintForm;
    }

    private void PrintForm(int[] answers)
    {
        int sheetNumber = forms.Count + 1;
        forms.Add(sheetNumber, answers);
        
    }

    [Button]
    public void ShowPrintedNotes()
    {
        showAnimation?.Kill();
        showAnimation = DOTween.Sequence();
        
        front.transform.localPosition = frontPos;
        back.transform.localPosition = backPos;
        
        showAnimation.Append(transform.DOMove(endTransform.position, .2f));
        showAnimation.Join(transform.DORotateQuaternion(endTransform.rotation, .2f));
        showAnimation.Join(transform.DOScale(endTransform.localScale, .2f));
        
        if (forms.Count == 0) return;
        
        currentForm = forms.Count;
        
        front.Initialize(currentForm, forms[currentForm]);
        front.gameObject.SetActive(true);

        if (forms.Count < 2) return;
        
        back.Initialize(currentForm - 1, forms[currentForm - 1]);
        back.gameObject.SetActive(true);
    }

    [Button]
    public void HidePrintedNotes()
    {
        showAnimation?.Kill();
        showAnimation = DOTween.Sequence();
        
        showAnimation.Append(transform.DOMove(startTransform.position, .2f));
        showAnimation.Join(transform.DORotateQuaternion(startTransform.rotation, .2f));
        showAnimation.Join(transform.DOScale(startTransform.localScale, .2f));
        showAnimation.OnComplete(() =>
        {
            front.gameObject.SetActive(false); 
            back.gameObject.SetActive(false);
        });
    }

    [Button]
    public void Next()
    {
        if (currentForm < forms.Count) currentForm++;
        else return;
        
        back.Initialize(currentForm, forms[currentForm]);
        
        Quaternion tiltRight = frontRot * Quaternion.Euler(0f, 10f, 10f);
        Quaternion tiltLeft = backRot * Quaternion.Euler(0f, -10f, -10f);
        
        switchAnimation?.Kill();
        switchAnimation = DOTween.Sequence();
        switchAnimation.SetEase(Ease.InOutQuad);
        
        // Move front right, back left
        switchAnimation.Append(front.transform.DOLocalMove(frontPos + Vector3.right * 0.2f, .4f));
        switchAnimation.Join(front.transform.DOLocalRotateQuaternion(tiltRight, 0.4f));
        switchAnimation.Join(back.transform.DOLocalMove(backPos + Vector3.left * 0.2f, .4f));
        switchAnimation.Join(back.transform.DOLocalRotateQuaternion(tiltLeft, 0.4f));
            
        // Swap positions
        switchAnimation.Append(front.transform.DOLocalMove(backPos, .4f));
        switchAnimation.Join(front.transform.DOLocalRotateQuaternion(backRot, 0.4f));
        switchAnimation.Join(back.transform.DOLocalMove(frontPos, .4f));
        switchAnimation.Join(back.transform.DOLocalRotateQuaternion(frontRot, 0.4f));

        switchAnimation.OnComplete(() => (front, back) = (back, front));
    }

    // TODO: Might be redundant and can be just one function for animation
    [Button]
    public void Previous()
    {
        if (currentForm - 1 > 0) currentForm--;
        else return;

        back.Initialize(currentForm, forms[currentForm]);

        Quaternion tiltRight = frontRot * Quaternion.Euler(0f, -10f, -10f);
        Quaternion tiltLeft = backRot * Quaternion.Euler(0f, 10f, 10f);

        switchAnimation?.Kill();
        switchAnimation = DOTween.Sequence();
        switchAnimation.SetEase(Ease.InOutQuad);

        // Move front left, back right
        switchAnimation.Append(front.transform.DOLocalMove(frontPos + Vector3.left * 0.2f, 0.4f));
        switchAnimation.Join(front.transform.DOLocalRotateQuaternion(tiltRight, 0.4f));
        switchAnimation.Join(back.transform.DOLocalMove(backPos + Vector3.right * 0.2f, 0.4f));
        switchAnimation.Join(back.transform.DOLocalRotateQuaternion(tiltLeft, 0.4f));

        // Swap positions
        switchAnimation.Append(front.transform.DOLocalMove(backPos, 0.4f));
        switchAnimation.Join(front.transform.DOLocalRotateQuaternion(backRot, 0.4f));
        switchAnimation.Join(back.transform.DOLocalMove(frontPos, 0.4f));
        switchAnimation.Join(back.transform.DOLocalRotateQuaternion(frontRot, 0.4f));

        switchAnimation.OnComplete(() => (front, back) = (back, front));
    }

}
