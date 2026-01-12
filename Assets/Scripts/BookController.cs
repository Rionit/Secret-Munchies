using System;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class BookController : MonoBehaviour
{
    public GameObject closedBook;
    public GameObject openedBook;

    private Vector3 ogClosedPosition;
    private Vector3 ogClosedRotation;
    private Vector3 ogOpenedPosition;
    //private Vector3 ogOpenedRotation;

    private bool isHeld = false;    
    private Sequence sequence;
    
    // Animation constants
    private const float LiftSmall = 0.1f;
    private const float LiftOpened = 0.33f;

    private const float LiftDurationShort = 0.2f;
    private const float LiftDurationMedium = 0.5f;

    private static readonly Vector3 GrabCameraOffset = new Vector3(0.1f, -0.55f, 0f);
    private static readonly Vector3 GrabRotation = new Vector3(-155f, -25f, -90f);
    
    private void Start()
    {
        GameManager.Instance.onCameraChanged += _ => PutDown();
        openedBook.SetActive(false);
    }

    public void Grab()
    {
        if (isHeld) return;
        
        
        ogClosedPosition = closedBook.transform.position;
        ogClosedRotation = closedBook.transform.eulerAngles;
        ogOpenedPosition = openedBook.transform.localPosition;
        //ogOpenedRotation = openedBook.transform.eulerAngles;

        sequence?.Kill();
        sequence = DOTween.Sequence();
        sequence.Append(closedBook.transform.DOMoveY(ogClosedPosition.y + LiftSmall, LiftDurationShort));
        sequence.Append(closedBook.transform.DOMove(GameManager.Instance.mainCamera.transform.position + GrabCameraOffset, LiftDurationMedium));
        sequence.Join(closedBook.transform.DORotate(GrabRotation, LiftDurationShort));
        sequence.Append(openedBook.transform.DOLocalMoveY(ogOpenedPosition.y + LiftOpened, LiftDurationMedium));

        isHeld = true;
        openedBook.SetActive(true);
    }

    public void PutDown()
    {
        if (!isHeld) return;
        
        sequence?.Kill();
        sequence = DOTween.Sequence();
        sequence.Append(openedBook.transform.DOLocalMove(ogOpenedPosition, LiftDurationShort));
        sequence.Append(closedBook.transform.DOMove(ogClosedPosition + new Vector3(0f, LiftSmall, 0f), LiftDurationMedium));
        sequence.Join(closedBook.transform.DORotate(ogClosedRotation, LiftDurationMedium));
        sequence.Append(closedBook.transform.DOMoveY(ogClosedPosition.y, LiftDurationShort));
        sequence.OnComplete(() => openedBook.SetActive(false));
        
        isHeld = false;
    }
}