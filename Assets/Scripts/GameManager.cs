using System;
using DG.Tweening;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public event Action<GameObject> onFoodGrabbed;
    public event Action onFoodDropped;
    public event Action onCameraChanged;
    
    [Header("Cinemachine Cameras (size = 4)")]
    public CinemachineCamera[] virtualCameras;
    public CinemachineCamera computerCloseupCamera;
    public Camera mainCamera;
    
    public int currentCustomerId = 0;
    public GameObject holdingFood {get; private set;}
    
    private int currentIndex = 0;
    private PlayerInput playerInput; // TODO: Move this to InputManager
    private CinemachineCamera overrideCamera;
    
    private void Awake()
    {
        // --- Singleton stuff ---
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
        SetActiveCamera(0); // Start with the first camera active
    }

    private void OnEnable()
    {
        // Bind input actions
        var actions = playerInput.actions;

        actions["CameraNext"].performed += _ => NextCamera();
        actions["CameraPrev"].performed += _ => PrevCamera();
    }

    private void OnDisable()
    {
        var actions = playerInput.actions;

        actions["CameraNext"].performed -= _ => NextCamera();
        actions["CameraPrev"].performed -= _ => PrevCamera();
    }

    private void NextCamera()
    {
        currentIndex = (currentIndex + 1) % virtualCameras.Length;
        SetActiveCamera(currentIndex);
        onCameraChanged?.Invoke();
    }

    private void PrevCamera()
    {
        currentIndex = (currentIndex - 1 + virtualCameras.Length) % virtualCameras.Length;
        SetActiveCamera(currentIndex);
        onCameraChanged?.Invoke();
    }
    
    private void SetActiveCamera(int index)
    {
        for (int i = 0; i < virtualCameras.Length; i++)
        {
            // Higher priority = active
            virtualCameras[i].Priority = (i == index) ? 10 : 0;
        }
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
        if (bag == null || holdingFood == null)
            return;
        bag.foods.Add(holdingFood.GetComponent<Food>().foodData);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(holdingFood.transform.DOMove(bag.transform.position + new Vector3(0, 0.2f, 0), 0.5f));
        sequence.Append(holdingFood.transform.DOMove(bag.transform.position, 0.5f)).OnComplete(() => Destroy(holdingFood)); 
        
        holdingFood = null;
        onFoodDropped?.Invoke();
    }

    public void HoldFood(GameObject instance)
    {
        holdingFood = instance;
        holdingFood.transform.DOMove(mainCamera.transform.position - new Vector3(0, 0.5f, 0), 0.5f);
        onFoodGrabbed?.Invoke(holdingFood);
    }
    
    public void OverrideActiveCamera(CinemachineCamera camera)
    {
        overrideCamera = camera;
        camera.Priority = 20;
    }

    public void ResetOverrideCamera()
    {
        overrideCamera.Priority = 0;
        overrideCamera = null;
    }
    
}