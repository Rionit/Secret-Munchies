using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeechBubble : MonoBehaviour
{
    [Title("References")]
    [Required] public RectTransform bubble;
    [Required] public TextMeshProUGUI text;

    [Title("Typing Settings")]
    [MinValue(0.01f)]
    public float typingSpeed = 0.04f;

    [MinValue(0.1f)]
    public float sentencePause = 0.4f;
    
    [Title("Tween Settings")]
    public float scaleTweenDuration = 0.25f;
    public Ease scaleEase = Ease.OutBack;

    [Title("Events")]
    public Action<string> OnCharacterTyped;
    public Action<string> OnTextTyped;
    public Action OnClosed;

    private Coroutine typingRoutine;

    private void Awake()
    {
        bubble.localScale = Vector3.zero;
    }

    private void Start()
    {
        OnCharacterTyped += AudioManager.Instance.OnCharacterTyped;
        AnimateIn();
    }

    [Button(ButtonSizes.Medium)]
    public void ShowText(string message)
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(TypeText(message));
    }

    [Button(ButtonSizes.Medium)]
    public void CloseAndDestroy()
    {
        AnimateOut(() =>
        {
            OnClosed?.Invoke();
            Destroy(gameObject);
        });
    }

    private void OnDestroy()
    {
        OnCharacterTyped -= AudioManager.Instance.OnCharacterTyped;
    }

    private IEnumerator TypeText(string message)
    {
        text.text = string.Empty;

        foreach (char c in message)
        {
            text.text += c;
            OnCharacterTyped?.Invoke(c.ToString());
            yield return new WaitForSeconds(c is '.' or ',' or '?' or '!' ? sentencePause : typingSpeed);
        }
        
        OnTextTyped?.Invoke(message);
    }

    private void AnimateIn()
    {
        bubble
            .DOScale(Vector3.one, scaleTweenDuration)
            .SetEase(scaleEase)
            .SetUpdate(true);
    }

    private void AnimateOut(Action onComplete = null)
    {
        bubble
            .DOScale(Vector3.zero, scaleTweenDuration)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => onComplete?.Invoke());
    }

}