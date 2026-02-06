using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class MorseCodePaperController : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Required] public MorseCodeController morseCodeController;
    [Required] public GameObject morseCodePaperPrefab;
    public float dragThreshold = 100f; // pixels

    private MorseCodePaper morseCodePaper;
    private Vector2 pointerDownPosition;
    private bool thresholdReached;

    private void Start()
    {
        morseCodeController.OnMorseCodeSymbolRegistered += OnMorseCodeSymbolRegistered;
        morseCodeController.OnMorseCodeCharacterRegistered += OnMorseCodeCharacterRegistered;
        morseCodePaper = Instantiate(morseCodePaperPrefab, transform.parent).GetComponent<MorseCodePaper>();
    }

    private void OnMorseCodeSymbolRegistered(string symbol)
    {
        morseCodePaper.morseCodeText.text += symbol;
    }

    private void OnMorseCodeCharacterRegistered(string symbol)
    {
        morseCodePaper.morseCodeText.text += " ";
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownPosition = eventData.position;
        thresholdReached = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float dragDistance = pointerDownPosition.y - eventData.position.y;

        if (dragDistance >= dragThreshold)
        {
            thresholdReached = true;
            // visual feedback ?
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (thresholdReached)
        {
            MorseCodePaper previous = morseCodePaper; 
            morseCodePaper = Instantiate(morseCodePaperPrefab, transform.parent).GetComponent<MorseCodePaper>();
            morseCodeController.FinishMessage();
            previous.transform.DOMove(previous.transform.position + Vector3.down * 700f + Vector3.right * Random.Range(50f, 350f), 2.0f);
            previous.transform.DORotate(new Vector3(0f, 0f, Random.Range(-10f, -90f)), 2.0f).OnComplete((() => Destroy(previous.gameObject)));
            
        }
    }
}
