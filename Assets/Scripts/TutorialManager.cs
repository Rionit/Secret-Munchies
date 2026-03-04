using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum TutorialTriggerType
{
    None,               // Auto advance after text
    OnStationChanged,   // When player switches virtual camera
    OnObjectClicked,    // When specific object clicked
    OnCustomEvent       // Manual trigger from code
}

[Serializable]
public class TutorialStep
{
    [TextArea(2, 5)]
    public string message;

    public TutorialTriggerType triggerType;

    [ShowIf(nameof(triggerType), TutorialTriggerType.OnStationChanged)]
    public int requiredStationIndex;

    [ShowIf(nameof(triggerType), TutorialTriggerType.OnObjectClicked)]
    public string requiredObjectId;

    [ShowIf(nameof(triggerType), TutorialTriggerType.OnCustomEvent)]
    public string customEventId;
}

public class TutorialManager : MonoBehaviour
{
    [Title("Speech Bubble")]
    [Required] public GameObject speechBubblePrefab;
    [Required] public Transform guiCanvas;

    public Image sceneTransition;

    [Title("Steps")]
    public List<TutorialStep> steps = new();

    private SpeechBubble bubbleInstance;
    private int currentStepIndex = 0;
    private TutorialStep CurrentStep => 
        currentStepIndex >= 0 && currentStepIndex < steps.Count 
        ? steps[currentStepIndex] 
        : null;

    private void Start()
    {
        GameManager.Instance.isTutorialActive = true;
        AudioManager.Instance.voicePitch = 0.7f;

        ShowStep(CurrentStep);
    }

    private void OnDestroy()
    {
        GameManager.Instance.isTutorialActive = false;
    }

    private void GoToNextStep()
    {
        currentStepIndex++;
    }

    private void ShowStep(TutorialStep step)
    {
        if (bubbleInstance != null)
            bubbleInstance.CloseAndDestroy();

        bubbleInstance = Instantiate(speechBubblePrefab, guiCanvas).GetComponent<SpeechBubble>();

        bubbleInstance.OnTextTyped += OnTextFinished;
        bubbleInstance.ShowText(step.message);

        GoToNextStep();
    }

    private void OnTextFinished(string message)
    {
        bubbleInstance.OnTextTyped -= OnTextFinished;
        bubbleInstance.OnClosed += OnBubbleClosed;
        bubbleInstance.CloseAndDestroy();
    }

    private void OnBubbleClosed()
    {
        if (bubbleInstance != null)
        {
            bubbleInstance.OnClosed -= OnBubbleClosed;
            bubbleInstance = null;
        }
        
        if (currentStepIndex >= steps.Count)
        {
            EndTutorial();
            return;
        }
        
        if (CurrentStep.triggerType == TutorialTriggerType.None)
        {
            ShowStep(CurrentStep);
        }
    }

    private void EndTutorial()
    {
        Debug.Log("Tutorial Finished");
        StartCoroutine(FadeToWhite());
    }

    private IEnumerator FadeToWhite()
    {
        float duration = 3f;
        float elapsed = 0f;

        GameManager.Instance.isTutorialActive = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            sceneTransition.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        sceneTransition.color = new Color(1f, 1f, 1f, 1f);
        SceneManager.LoadScene("Main");
    }

    public void OnStationChanged(int stationIndex)
    {
        if (CurrentStep == null) return;

        if (CurrentStep.triggerType == TutorialTriggerType.OnStationChanged &&
            CurrentStep.requiredStationIndex == stationIndex)
        {
            ShowStep(CurrentStep);
        }
    }

    public void OnObjectClicked(string objectId)
    {
        if (CurrentStep == null) return;

        if (CurrentStep.triggerType == TutorialTriggerType.OnObjectClicked &&
            CurrentStep.requiredObjectId == objectId)
        {
            ShowStep(CurrentStep);
        }
    }

    public void TriggerCustomEvent(string eventId)
    {
        if (CurrentStep == null) return;

        if (CurrentStep.triggerType == TutorialTriggerType.OnCustomEvent &&
            CurrentStep.customEventId == eventId)
        {
            ShowStep(CurrentStep);
        }
    }

}