using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.isTutorialActive = true;
    }

    private void OnDestroy()
    {
        GameManager.Instance.isTutorialActive = false;
    }
}
