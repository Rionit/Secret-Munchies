using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Cinemachine;

public class DialogueController : MonoBehaviour
{
    [Title("Speech Bubble")]
    [Required] public GameObject speechBubblePrefab;
    [Required] public Transform guiCanvas;

    [Title("Dialogue Scriptable Objects")]
    [Required] public DialogueMessageSO openerSO;
    [Required] public DialogueMessageSO orderSO;
    [Required] public DialogueMessageSO chitChatSO;
    [Required] public DialogueMessageSO topSecretSO;

    [Title("Settings")]
    [MinValue(0f)] public float delayBetweenMessages = 0.6f;
    [Range(0f, 1f)] public float extraMessageChance = 0.4f;
    [Range(0f, 1f)] public float topSecretChance = 0.15f;

    [Title("Camera Settings")]
    [Required] public CinemachineCamera properCamera; 

    // Fired when dialogue ends. Parameter is the used TopSecretCategory (None if none was used)
    public event Action<TopSecretCategory> OnDialogueEnded;

    private Queue<DialogueMessage> messageQueue = new(); 
    private Queue<DialogueMessage> backupQueue = new(); 
    private SpeechBubble bubbleInstance;
    private NPC currentNPC;
    private bool isDialogueBuilt = false;
    private bool isProperCamera = false;
    private Coroutine currentDelayCoroutine; 

    private TopSecretCategory usedTopSecretCategory = TopSecretCategory.None;
    private bool isCurrentMessageFromOrder = false;
    
    // Helper class to store message with its type
    private class DialogueMessage
    {
        public string text;
        public DialogueMessageSO.MessageType type;
        
        public DialogueMessage(string text, DialogueMessageSO.MessageType type)
        {
            this.text = text;
            this.type = type;
        }
    }

    private void Start()
    {
        GameManager.Instance.onCameraChanged += OnCameraChanged;
        GameManager.Instance.onCameraBlendFinished += OnCameraBlendFinished;
        AIManager.Instance.onNPCOrderCreated += OnOrderFinished;
    }

    private void OnDisable()
    {
        GameManager.Instance.onCameraChanged -= OnCameraChanged;
        GameManager.Instance.onCameraBlendFinished -= OnCameraBlendFinished;
        AIManager.Instance.onNPCOrderCreated -= OnOrderFinished;

        if (currentDelayCoroutine != null)
        {
            StopCoroutine(currentDelayCoroutine);
            currentDelayCoroutine = null;
        }
    }

    public void StartDialogue(NPC npc)
    {
        currentNPC = npc;
        
        if (!isProperCamera) return;
        
        ClearCurrentDialogue();

        if (!isDialogueBuilt)
        {
            messageQueue.Clear();
            usedTopSecretCategory = TopSecretCategory.None;

            BuildDialogue(npc);
        }

        bubbleInstance = Instantiate(speechBubblePrefab, guiCanvas).GetComponent<SpeechBubble>();
        bubbleInstance.OnTextTyped += OnMessageFinished;

        ShowNextMessage();
    }

    private void BuildDialogue(NPC npc)
    {
        isDialogueBuilt = false;
        
        AddMessages(openerSO, npc);
        AddMessages(orderSO, npc);

        if (UnityEngine.Random.value <= extraMessageChance)
        {
            if (UnityEngine.Random.value <= topSecretChance)
                AddMessages(topSecretSO, npc);
            else
                AddMessages(chitChatSO, npc);
        }

        backupQueue = new Queue<DialogueMessage>(messageQueue);
        isDialogueBuilt = true;
    }

    private void AddMessages(DialogueMessageSO messageSO, NPC npc)
    {
        if (messageSO == null) return;

        if (messageSO.type == DialogueMessageSO.MessageType.TOP_SECRET)
        {
            if (messageSO.variations == null || messageSO.variations.Count == 0)
                return;

            DialogueVariation variation =
                messageSO.variations[UnityEngine.Random.Range(0, messageSO.variations.Count)];

            usedTopSecretCategory = variation.categories;

            foreach (var msg in variation.sequence)
                messageQueue.Enqueue(new DialogueMessage(msg, messageSO.type));

            return;
        }
        
        Debug.Log(messageSO.type);

        foreach (var msg in messageSO.GetRandomVariation(npc.wantedFoods))
            messageQueue.Enqueue(new DialogueMessage(msg, messageSO.type));
    }

    private void ShowNextMessage()
    {
        /*
         * TODO: Maybe wait for order to be made in the computer
         * and only then show chit chat/top secret?
         *
         * Also possible improvement: Restart from unfinished
         * message?
         */
        
        if (messageQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        var nextMessage = messageQueue.Dequeue();
        isCurrentMessageFromOrder = (nextMessage.type == DialogueMessageSO.MessageType.ORDER);
        bubbleInstance.ShowText(nextMessage.text);
    }

    private void OnMessageFinished(String message)
    {
        currentDelayCoroutine = StartCoroutine(NextMessageDelayed());
    }

    private IEnumerator NextMessageDelayed()
    {
        float delay = isCurrentMessageFromOrder ? delayBetweenMessages * 2f : delayBetweenMessages;
        yield return new WaitForSeconds(delay);
        ShowNextMessage();
        currentDelayCoroutine = null;
    }

    private void EndDialogue()
    {
        ClearCurrentDialogue();
        OnDialogueEnded?.Invoke(usedTopSecretCategory);
    }

    private void OnOrderFinished()
    {
        backupQueue.Clear();
        isDialogueBuilt = false;
    }
    
    private void OnCameraChanged(CinemachineCamera newCamera)
    {
        ClearCurrentDialogue();
        isProperCamera = false;
    }

    private void OnCameraBlendFinished(CinemachineCamera newCamera)
    {
        Debug.LogWarning("Hello everybody my name is Markiplier");
        isProperCamera = newCamera == properCamera;
        if (isProperCamera)
        {
            Debug.LogWarning("Yup, proper camera");
            if (currentNPC != null)
            {
                Debug.LogWarning("Camera changed back - restarting dialogue");
                RestartDialogue();
            }
        }
    }

    private void ClearCurrentDialogue()
    {
        if (currentDelayCoroutine != null)
        {
            StopCoroutine(currentDelayCoroutine);
            currentDelayCoroutine = null;
        }
        
        if (bubbleInstance != null)
        {
            bubbleInstance.OnTextTyped -= OnMessageFinished;
            bubbleInstance.CloseAndDestroy();
            bubbleInstance = null;
        }
    }

    [Button("Restart Dialogue")]
    private void RestartDialogue()
    {
        if (currentNPC != null)
        {
            Debug.Log("Restarting dialogue");
            messageQueue = new Queue<DialogueMessage>(backupQueue);
            StartDialogue(currentNPC);
        }
    }
}