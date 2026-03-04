using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public Image itemImage;
    public GameObject itemFrame;
    public Sprite bagSprite;
    
    public GameObject morseCodeUI;
    
    public GameObject maineMenu;
    public GameObject gameName;
    public GameObject morseCodeTop;
    public GameObject morseCodeBottom;
    
    private RectTransform gameNameRT;
    private RectTransform topRT;
    private RectTransform bottomRT;

    private Vector2 gameNameFinalPos;
    private Vector2 topFinalPos;
    private Vector2 bottomFinalPos;

    private Sequence menuSequence;

    private const float offsetX = 900f;
    private const float offsetY = 300f;
    private const float duration = 1.5f;
    
    private void Awake()
    {
        maineMenu.SetActive(true);
        
        gameNameRT = gameName.GetComponent<RectTransform>();
        topRT = morseCodeTop.GetComponent<RectTransform>();
        bottomRT = morseCodeBottom.GetComponent<RectTransform>();

        gameNameFinalPos = gameNameRT.anchoredPosition;
        topFinalPos = topRT.anchoredPosition;
        bottomFinalPos = bottomRT.anchoredPosition;

        HideInstant();
    }
    
    private void Start()
    {
        GameManager.Instance.onItemGrabbed += OnItemGrabbed;
        GameManager.Instance.onItemDropped += OnItemDropped;
        GameManager.Instance.onMenuSwitched += OneMenuSwitched;
    }

    private void OnDestroy()
    {
        GameManager.Instance.onItemGrabbed -= OnItemGrabbed;
        GameManager.Instance.onItemDropped -= OnItemDropped;
        GameManager.Instance.onMenuSwitched -= OneMenuSwitched;
    }

    private void OnItemGrabbed(GameObject obj)
    {
        Food food = obj.GetComponent<Food>();
        if (food != null)
            itemImage.sprite = food.foodData.sprite;
        else
            itemImage.sprite = bagSprite;
        
        itemFrame.gameObject.SetActive(true);
    }
    
    private void OnItemDropped(GameObject obj)
    {
        itemImage.sprite = null;
        itemFrame.SetActive(false);
    }

    private void HideInstant()
    {
        gameNameRT.anchoredPosition = gameNameFinalPos + Vector2.right * offsetX;
        topRT.anchoredPosition = topFinalPos + Vector2.up * offsetY;
        bottomRT.anchoredPosition = bottomFinalPos + Vector2.down * offsetY;
    }

    public void OneMenuSwitched(bool isMenuActive)
    {
        // Kill active tween
        menuSequence?.Kill();

        menuSequence = DOTween.Sequence();
        menuSequence.SetUpdate(true);

        if (isMenuActive)
        {
            menuSequence
                .Join(gameNameRT.DOAnchorPos(gameNameFinalPos, duration).SetEase(Ease.OutCubic))
                .Join(topRT.DOAnchorPos(topFinalPos, duration).SetEase(Ease.OutCubic))
                .Join(bottomRT.DOAnchorPos(bottomFinalPos, duration).SetEase(Ease.OutCubic));
        }
        else
        {
            menuSequence
                .Join(gameNameRT.DOAnchorPos(gameNameFinalPos + Vector2.right * offsetX, duration / 2f).SetEase(Ease.InCubic))
                .Join(topRT.DOAnchorPos(topFinalPos + Vector2.up * offsetY, duration / 2f).SetEase(Ease.InCubic))
                .Join(bottomRT.DOAnchorPos(bottomFinalPos + Vector2.down * offsetY, duration / 2f).SetEase(Ease.InCubic));
        }
    }

    public void UpdateMorseCodeUI()
    {
        Debug.Log("UpdateMorseCodeUI");
        morseCodeUI.SetActive(!morseCodeUI.activeSelf);
        GameManager.Instance.morseCodeController.isActive = morseCodeUI.activeSelf;
        if(morseCodeUI.activeSelf)
            AudioManager.Instance.FadeIn("morse_code_background_hum");
        else
            AudioManager.Instance.FadeOut("morse_code_background_hum");
    }
}
