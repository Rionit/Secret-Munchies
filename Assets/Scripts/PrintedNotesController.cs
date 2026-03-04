using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public struct FormData
{
    public int[] masks;
    public bool[] censorships;

    public FormData(int[] masks)
    {
        this.masks = masks;
        censorships = new bool[masks.Length];
    }
}

public class PrintedNotesController : MonoBehaviour
{
    [Required] public Transform startTransform;
    [Required] public Transform endTransform;

    public PrintedForm formA;
    public PrintedForm formB;

    [ShowInInspector]
    private List<FormData> forms = new();

    [ShowInInspector]
    private int currentFormIndex = -1; // 0-based

    [ShowInInspector] private PrintedForm front;
    [ShowInInspector] private PrintedForm back;

    private Vector3 frontPos;
    private Vector3 backPos;
    private Quaternion frontRot;
    private Quaternion backRot;

    private Sequence switchAnimation;
    private Sequence showAnimation;

    private void Start()
    {
        formA.OnFormDataChanged += OnFormDataChanged;
        formB.OnFormDataChanged += OnFormDataChanged;

        front = formA;
        back = formB;

        frontPos = front.transform.localPosition;
        backPos = back.transform.localPosition;
        frontRot = front.transform.localRotation;
        backRot = back.transform.localRotation;

        HidePrintedNotes();
        GameManager.Instance.notepadApp.OnPrintForm += PrintForm;
    }

    private void OnFormDataChanged(FormData formData, int formNumber)
    {
        forms[formNumber] = formData;
    }

    private void PrintForm(int[] answers)
    {
        forms.Add(new FormData(answers));
        AudioManager.Instance.PlayOneShot("printer");
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        front.gameObject.SetActive(forms.Count >= 1);
        back.gameObject.SetActive(forms.Count >= 2);
    }

    [Button]
    public void ShowPrintedNotes()
    {
        UpdateVisibility();
        showAnimation?.Kill();
        showAnimation = DOTween.Sequence();

        front.transform.localPosition = frontPos;
        back.transform.localPosition = backPos;

        showAnimation.Append(transform.DOMove(endTransform.position, .2f));
        showAnimation.Join(transform.DORotateQuaternion(endTransform.rotation, .2f));
        showAnimation.Join(transform.DOScale(endTransform.localScale, .2f));

        if (forms.Count == 0) return;
        currentFormIndex = forms.Count - 1;
        front.Initialize(currentFormIndex + 1, forms[currentFormIndex]);

        if (forms.Count < 2) return;
        back.Initialize(currentFormIndex, forms[currentFormIndex - 1]);
    }

    [Button]
    public void HidePrintedNotes()
    {
        UpdateVisibility();
        showAnimation?.Kill();
        showAnimation = DOTween.Sequence();

        showAnimation.Append(transform.DOMove(startTransform.position, .2f));
        showAnimation.Join(transform.DORotateQuaternion(startTransform.rotation, .2f));
        showAnimation.Join(transform.DOScale(startTransform.localScale, .2f));
    }

    [Button]
    public void Next()
    {
        if (currentFormIndex >= forms.Count - 1) return;
        currentFormIndex++;

        back.Initialize(currentFormIndex + 1, forms[currentFormIndex]);

        AnimateSwap(forward: true);
    }

    [Button]
    public void Previous()
    {
        if (currentFormIndex <= 0) return;
        currentFormIndex--;

        back.Initialize(currentFormIndex + 1, forms[currentFormIndex]);

        AnimateSwap(forward: false);
    }

    private void AnimateSwap(bool forward)
    {
        Quaternion tiltFront = frontRot * Quaternion.Euler(0f, forward ? 10f : -10f, forward ? 10f : -10f);
        Quaternion tiltBack  = backRot  * Quaternion.Euler(0f, forward ? -10f : 10f, forward ? -10f : 10f);

        AudioManager.Instance.PlayOneShot("paper_shuffle");

        switchAnimation?.Kill();
        switchAnimation = DOTween.Sequence();
        switchAnimation.SetEase(Ease.InOutQuad);

        Vector3 frontOffset = Vector3.right * (forward ? 0.2f : -0.2f);
        Vector3 backOffset  = Vector3.left  * (forward ? 0.2f : -0.2f);

        switchAnimation.Append(front.transform.DOLocalMove(frontPos + frontOffset, .4f));
        switchAnimation.Join(front.transform.DOLocalRotateQuaternion(tiltFront, .4f));
        switchAnimation.Join(back.transform.DOLocalMove(backPos + backOffset, .4f));
        switchAnimation.Join(back.transform.DOLocalRotateQuaternion(tiltBack, .4f));

        switchAnimation.Append(front.transform.DOLocalMove(backPos, .4f));
        switchAnimation.Join(front.transform.DOLocalRotateQuaternion(backRot, .4f));
        switchAnimation.Join(back.transform.DOLocalMove(frontPos, .4f));
        switchAnimation.Join(back.transform.DOLocalRotateQuaternion(frontRot, .4f));

        switchAnimation.OnComplete(() => (front, back) = (back, front));
    }
}
