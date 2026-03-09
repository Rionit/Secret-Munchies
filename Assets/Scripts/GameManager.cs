using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

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
    [HideInInspector] public event Action<int> onSecretHeartsChanged;
    [HideInInspector] public event Action<int> onOrderHeartsChanged;
    [HideInInspector] public event Action onGameOver;

    [Title("Cameras")]
    [ValidateInput(nameof(ValidateVirtualCameras),
        "Virtual Cameras array must have at least 4 basic virtual cameras (pc, packing, handout, morse)",
        InfoMessageType.Error)]
    [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    public VirtualCamera[] virtualCameras;

    [Required, Tooltip("Camera overlooking Pentagon, used for Main Menu")]
    public CinemachineCamera pentagonCamera;
    
    [Required, Tooltip("Camera looking into blank space, used for Start of the game")]
    public CinemachineCamera blankViewCamera;

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
    public MorseCodeController morseCodeController;

    [Required]
    public NotepadApp notepadApp;
    
    [ShowInInspector, LabelText("Current Camera Index"), ValueDropdown(nameof(GetCameraIndices))]
    public int currentCameraIdx = 0;

    public VirtualCamera currentVirtualCamera;
    
    private IEnumerable<int> GetCameraIndices()
    {
        if (virtualCameras == null) yield break;
        for (int i = 0; i < virtualCameras.Length; i++)
            yield return i;
    }
    
    [ReadOnly, ShowInInspector]
    public PlayerInput playerInput;

    [ReadOnly, ShowInInspector]
    private CinemachineCamera overrideCamera;
    
    private CinemachineBrain cinemachineBrain;
    private Coroutine currentBlend;

    [ShowInInspector] private List<Secret> secrets = new List<Secret>();
    
    private bool isMenuActive = false;

    public bool isTutorialActive;

    public int secretsHearts { get; private set; } = 3;
    public int ordersHearts { get; private set; } = 3;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "Main")
        {
            Destroy(Instance);
            Instance = this;            
            //DontDestroyOnLoad(gameObject);
        }
        
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
        
        if (AIManager.Instance != null)
            AIManager.Instance.dialogueController.OnDialogueEnded += OnDialogueEnded;
        
        SetActiveCamera(0);

        if (SceneManager.GetActiveScene().name == "Main")
        {
            Time.timeScale = 0f;
            StartCoroutine(OverrideThenToggle());
            AudioManager.Instance.Play("kitchen");
            morseCodeController.OnMessageFinished += CheckMorseWithSecrets;
        }
    }

    public void Reset()
    {
        SceneManager.LoadScene("Main");
    }

    IEnumerator OverrideThenToggle()
    {
        OverrideActiveCamera(blankViewCamera);
        yield return new WaitForSecondsRealtime(1f); 
        ToggleMenu();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCamera = Camera.main;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        morseCodeController.OnMessageFinished -= CheckMorseWithSecrets;
    }
    
    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        if (morseCodeController.isActive) return;
        
        Vector2 input = ctx.ReadValue<Vector2>();

        if (input.magnitude < 0.5f)
            return;

        // Dominant axis only (prevents diagonal double calls)
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            if (input.x > 0)
                currentVirtualCamera.D();
            else
                currentVirtualCamera.A();
        }
        else
        {
            if (input.y > 0)
                currentVirtualCamera.W();
            else
                currentVirtualCamera.S();
        }
    }

    private void OnEnable()
    {
        var actions = playerInput.actions;
        //actions["CameraNext"].performed += _ => NextCamera();
        //actions["CameraPrev"].performed += _ => PrevCamera();
        actions["Menu"].performed += _ => ToggleMenu();
        actions["Dot"].performed += _ => morseCodeController.Dot();
        actions["Dash"].performed += _ => morseCodeController.Dash();
        actions["Move"].performed += OnMovePerformed;
    }

    private void OnDisable()
    {
        var actions = playerInput.actions;
        //actions["CameraNext"].performed -= _ => NextCamera();
        //actions["CameraPrev"].performed -= _ => PrevCamera();
        actions["Menu"].performed -= _ => ToggleMenu();
        actions["Dot"].performed -= _ => morseCodeController.Dot();
        actions["Dash"].performed -= _ => morseCodeController.Dash();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ToggleMenu()
    {
        if (isTutorialActive)
        {
            SceneManager.LoadScene("Main");
            isTutorialActive = false;
            return;
        }
        
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
            if(AIManager.Instance != null)
                AIManager.Instance.dialogueController.ClearCurrentDialogue();
            OverrideActiveCamera(pentagonCamera);
            onCameraBlendFinished += _ =>
            {
                Time.timeScale = 0f;
                onMenuSwitched?.Invoke(isMenuActive);
            };
        }
        
    }

    public void ChangeCamera(VirtualCamera virtualCamera)
    {
        currentCameraIdx = Array.IndexOf(virtualCameras, virtualCamera);
        SetActiveCamera(currentCameraIdx);
        onCameraChanged?.Invoke(virtualCameras[currentCameraIdx].thisCamera);
        currentVirtualCamera = virtualCamera;
    }
    
    public void SetActiveStation(int index)
    {
        TutorialManager tutorial = FindObjectOfType<TutorialManager>();
        if (tutorial != null)
            tutorial.OnStationChanged(index);
    }
    
    public void NotifyTutorial(string eventName)
    {
        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            TutorialManager tutorial = FindObjectOfType<TutorialManager>();
            tutorial.TriggerCustomEvent(eventName);
        }
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
        onCameraBlendFinished?.Invoke(overrideCamera != null ? overrideCamera : virtualCameras[currentCameraIdx].thisCamera);
        
        if (SceneManager.GetActiveScene().name == "Tutorial")
            SetActiveStation(currentCameraIdx);
    }

    private void SetActiveCamera(int index)
    {
        for (int i = 0; i < virtualCameras.Length; i++)
        {
            virtualCameras[i].thisCamera.Priority = (i == index) ? 10 : 0;
            virtualCameras[i].isActive = (i == index);
        }
     
        if (overrideCamera == null)
            StartWaitingForCameraBlendFinish();
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

    public void CheckMorseWithSecrets(string message)
    {
        foreach (Secret secret in secrets)
        {
            if(Secret.GetSecretCategory(message) == secret.category)
            {
                return;
            }
        }

        secretsHearts--;
        onSecretHeartsChanged?.Invoke(secretsHearts);
        if (secretsHearts <= 0)
        {
            onGameOver?.Invoke();
            Time.timeScale = 0.0f;
        }
    }

    public void DecreaseOrderHearts()
    {
        ordersHearts--;
        onOrderHeartsChanged?.Invoke(ordersHearts);
        if (ordersHearts <= 0)
        {
            onGameOver?.Invoke();
            Time.timeScale = 0.0f;
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
        if (camera == null) return;
        ResetOverrideCamera();
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

    private bool ValidateVirtualCameras(VirtualCamera[] cams)
    {
        return cams != null && cams.Length >= 4;
    }
}
