using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class VirtualCamera : SerializedMonoBehaviour
{
    public Action<VirtualCamera> OnNextVirtualCameraSelected;
    
    public bool isActive = false;

    public CinemachineCamera thisCamera;

    public VirtualCamera frontCamera; // W (top row, middle)
    public VirtualCamera leftCamera; // A (bottom row, left)
    public VirtualCamera backCamera; // S (bottom row, middle)
    public VirtualCamera rightCamera; // D (bottom row, right)
    private void Start()
    {
        thisCamera = GetComponent<CinemachineCamera>();
        OnNextVirtualCameraSelected += GameManager.Instance.ChangeCamera;
    }

    public void W()
    {
        if (!isActive || !frontCamera) return;
        OnNextVirtualCameraSelected?.Invoke(frontCamera);
    }

    public void A()
    {
        if (!isActive || !leftCamera) return;
        OnNextVirtualCameraSelected?.Invoke(leftCamera);
    }

    public void S()
    {
        if (!isActive || !backCamera) return;
        OnNextVirtualCameraSelected?.Invoke(backCamera);
    }

    public void D()
    {
        if (!isActive || !rightCamera) return;
        OnNextVirtualCameraSelected?.Invoke(rightCamera);
    }

    private void OnDisable()
    {
        OnNextVirtualCameraSelected -= GameManager.Instance.ChangeCamera;
    }
}