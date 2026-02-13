using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Title("Events")]
    [HideInInspector] public event Action<GameObject> onItemGrabbed;
    [HideInInspector] public event Action<GameObject> onItemDropped;
    [HideInInspector] public event Action<GameObject> onItemDropTweenEnded;
    [HideInInspector] public event Action<CinemachineCamera> onCameraChanged;
    [HideInInspector] public event Action<CinemachineCamera> onCameraBlendFinished;
    [HideInInspector] public event Action<bool> onMenuSwitched;

    [Title("Cameras")]
    [ValidateInput(nameof(ValidateVirtualCameras),
        "Virtual Cameras array must have exactly 4 elements",
        InfoMessageType.Error)]
    [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    public CinemachineCamera[] virtualCameras;

    [Required, Tooltip("Close-up camera used for computer interaction")]
    public CinemachineCamera computerCloseupCamera;

    [Required, Tooltip("Camera overlooking Pentagon, used for Main Menu")]
    public CinemachineCamera pentagonCamera;

    [Required]
    public Camera mainCamera;

    [Title("Player Interaction")]
    [ReadOnly, Tooltip("Current customer being served")]
    public int currentCustomerId = 0;

    [ReadOnly, ShowInInspector, Tooltip("Currently held item")]
    public GameObject holdingItem { get; private set; }

    [Required]
    public GameObject hand;

    [Required]
    public CounterController counterController;

    [Required]
    public NotepadApp notepadApp;
    
    [ShowInInspector, LabelText("Current Camera Index"), ValueDropdown(nameof(GetCameraIndices))]
    public int currentCameraIdx = 0;

    private IEnumerable<int> GetCameraIndices()
    {
        if (virtualCameras == null) yield break;
        for (int i = 0; i < virtualCameras.Length; i++)
            yield return i;
    }
    
    [ReadOnly, ShowInInspector]
    private PlayerInput playerInput;

    [ReadOnly, ShowInInspector]
    private CinemachineCamera overrideCamera;
    
    private CinemachineBrain cinemachineBrain;
    private Coroutine currentBlend;

    [ShowInInspector] private List<Secret> secrets = new List<Secret>();
    
    private bool isMenuActive;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
        
        SetActiveCamera(0);
        onCameraBlendFinished?.Invoke(virtualCameras[0]); // if this camera already active 
        
        AIManager.Instance.dialogueController.OnDialogueEnded += OnDialogueEnded;
    }

    private void OnEnable()
    {
        var actions = playerInput.actions;
        actions["CameraNext"].performed += _ => NextCamera();
        actions["CameraPrev"].performed += _ => PrevCamera();
        actions["Menu"].performed += _ => ToggleMenu();
    }

    private void OnDisable()
    {
        var actions = playerInput.actions;
        actions["CameraNext"].performed -= _ => NextCamera();
        actions["CameraPrev"].performed -= _ => PrevCamera();
        actions["Menu"].performed -= _ => ToggleMenu();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ToggleMenu()
    {
        if (isMenuActive)
        {
            isMenuActive = false;
            AudioManager.Instance.FadeOut("music_menu", 5f);
            CinemachineCore.UniformDeltaTimeOverride = Time.unscaledDeltaTime;
            ResetOverrideCamera();
            onCameraBlendFinished += _ =>
            {
                Time.timeScale = 1f;
            };
            onMenuSwitched?.Invoke(isMenuActive);
        }
        else
        {
            isMenuActive = true;
            AudioManager.Instance.FadeIn("music_menu", 5f);
            CinemachineCore.UniformDeltaTimeOverride = Time.unscaledDeltaTime;
            AIManager.Instance.dialogueController.ClearCurrentDialogue();
            OverrideActiveCamera(pentagonCamera);
            onCameraBlendFinished += _ =>
            {
                Time.timeScale = 0f;
                onMenuSwitched?.Invoke(isMenuActive);
            };
        }
        
    }

    [Button(ButtonSizes.Small)]
    private void NextCamera()
    {
        currentCameraIdx = (currentCameraIdx + 1) % virtualCameras.Length;
        SetActiveCamera(currentCameraIdx);
        onCameraChanged?.Invoke(virtualCameras[currentCameraIdx]);
        Debug.LogWarning("Next Camera");
    }

    [Button(ButtonSizes.Small)]
    private void PrevCamera()
    {
        currentCameraIdx = (currentCameraIdx - 1 + virtualCameras.Length) % virtualCameras.Length;
        SetActiveCamera(currentCameraIdx);
        onCameraChanged?.Invoke(virtualCameras[currentCameraIdx]);
        Debug.LogWarning("Prev Camera");
    }

    private void StartWaitingForCameraBlendFinish()
    {
        // Stop previous blend if running
        if (currentBlend != null)
        {
            StopCoroutine(currentBlend);
        }
        
        // Start new blend wait
        currentBlend = StartCoroutine(WaitForCameraBlendFinish());
    }
    
    private IEnumerator WaitForCameraBlendFinish()
    {
        Debug.Log("Starting camera switch...");
        yield return new WaitUntil(() => cinemachineBrain.IsBlending);
        
        Debug.Log("Blend in progress...");
        
        // Wait for blend to complete
        yield return new WaitWhile(() => cinemachineBrain.IsBlending);
        
        Debug.Log("Blend complete! Camera transition finished.");
        onCameraBlendFinished?.Invoke(overrideCamera != null ? overrideCamera : virtualCameras[currentCameraIdx]);
    }

    private void SetActiveCamera(int index)
    {
        for (int i = 0; i < virtualCameras.Length; i++)
            virtualCameras[i].Priority = (i == index) ? 10 : 0;
        StartWaitingForCameraBlendFinish();
    }

    public void OnComputerClick(GameObject sender)
    {
        if (overrideCamera == null)
            OverrideActiveCamera(computerCloseupCamera);
        else
            ResetOverrideCamera();
    }

    public void OnOrderBagClick(GameObject sender)
    {
        Bag bag = sender.GetComponent<Bag>();
        if (bag == null || holdingItem == null) return;

        Food food = holdingItem.GetComponent<Food>();
        if (food == null) return;

        bag.AddFood(food.foodData);
        GameObject itemToDestroy = holdingItem;
        
        AudioManager.Instance.PlayOneShot("deep_woosh");

        Sequence sequence = DOTween.Sequence();
        sequence.Append(holdingItem.transform.DOMove(bag.transform.position + Vector3.up * 0.2f, 0.5f));
        sequence.Append(holdingItem.transform.DOMove(bag.transform.position, 0.5f))
            .OnComplete(() =>
            {
                Destroy(itemToDestroy);
                onItemDropTweenEnded?.Invoke(holdingItem);
                bag.PlayAddFoodTween();
            });

        onItemDropped?.Invoke(holdingItem);
        holdingItem = null;
    }

    private void OnDialogueEnded(TopSecretCategory category)
    {
        if (category != TopSecretCategory.None)
        {
            Secret secret = new Secret(category);
            secrets.Add(secret);
        }
    }

    public bool GrabItem(GameObject instance)
    {
        if (holdingItem != null) return false;

        holdingItem = instance;

        AudioManager.Instance.PlayOneShot("high_woosh");

        Sequence sequence = DOTween.Sequence();
        sequence.Append(holdingItem.transform.DOMoveY(holdingItem.transform.position.y + 0.1f, 0.2f));
        sequence.Append(holdingItem.transform.DOMove(hand.transform.position, 0.5f))
            .OnComplete(() => holdingItem?.transform.SetParent(mainCamera.transform));

        onItemGrabbed?.Invoke(holdingItem);
        return true;
    }

    public void DropItem(Vector3 position)
    {
        if (holdingItem == null) return;

        GameObject droppedItem = holdingItem;

        AudioManager.Instance.PlayOneShot("deep_woosh");

        Sequence sequence = DOTween.Sequence();
        sequence.Append(holdingItem.transform.DOMove(position, 0.5f))
            .OnComplete(() =>
            {
                droppedItem.transform.SetParent(null);
                onItemDropped?.Invoke(droppedItem); // TODO: Put outside lambda
                onItemDropTweenEnded?.Invoke(holdingItem);
            });

        holdingItem = null;
    }

    public void OverrideActiveCamera(CinemachineCamera camera)
    {
        overrideCamera = camera;
        camera.Priority = 20;
        StartWaitingForCameraBlendFinish();
    }

    public void ResetOverrideCamera()
    {
        if (overrideCamera == null) return;
        overrideCamera.Priority = 0;
        overrideCamera = null;
        StartWaitingForCameraBlendFinish();
    }

    private bool ValidateVirtualCameras(CinemachineCamera[] cams)
    {
        return cams != null && cams.Length == 4;
    }
}
