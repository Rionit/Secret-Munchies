using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class Bag : MonoBehaviour
{
    public GameObject bagOpen;
    public GameObject bagClosed;
    
    public ParticleSystem bagParticles;
    public TextMeshProUGUI orderNumberText;
    public RectTransform progressBar;
    public float progressBarSpeed = 4f;

    public List<FoodAmount> foods = new List<FoodAmount>();
    public int orderId;

    [SerializeField] private bool isPacked;
    private bool isMouseDown;

    private int spawnId;
    private bool initialized = false;
    private const float progressBardWidth = 0.1362f;

    [Header("Add Food Tween")]
    public float punchScale = 0.15f;
    public float punchPosition = 0.1f;
    public float tweenDuration = 0.25f;

    private Vector3 initialScale;
    private Vector3 initialPosition;
    private Tween activeTween;

    private void Awake()
    {
        initialScale = transform.localScale;
        initialPosition = transform.localPosition;
    }

    public void Initialize(int spawnId, int orderId)
    {
        SetPacked(false);
        GetComponent<Clickable>().onClick.AddListener(GameManager.Instance.OnOrderBagClick);
        progressBar.parent.gameObject.SetActive(false);
        this.spawnId = spawnId;
        this.orderId = orderId;
        orderNumberText.text = this.orderId.ToString();
        progressBar.sizeDelta = new Vector2(0.0f, progressBar.sizeDelta.y);
        initialized = true;
    }

    private void Update()
    {
        if (isPacked || !initialized)
            return;

        if (foods.Count != 0)
            progressBar.parent.gameObject.SetActive(true);

        float x = Mathf.Lerp(
            progressBar.sizeDelta.x,
            isMouseDown ? progressBardWidth : 0f,
            Time.deltaTime * progressBarSpeed
        );

        progressBar.sizeDelta = new Vector2(x, progressBar.sizeDelta.y);

        if (x >= progressBardWidth - 0.01f)
        {
            SetPacked(GameManager.Instance.GrabItem(gameObject));
        }
    }

    [Button]
    private void SetPacked(bool packed)
    {
        if (isPacked == packed)
            return;

        isPacked = packed;

        bagOpen.SetActive(!isPacked);
        bagClosed.SetActive(isPacked);
        progressBar.parent.gameObject.SetActive(!isPacked);

        if (isPacked) FoodManager.Instance?.FreeSpawnPoint(spawnId);
    }

    public void AddFood(FoodSO food, int amount = 1)
    {
        if (amount <= 0) return;

        FoodAmount existing = foods.Find(f => f.food == food);
        if (existing != null)
        {
            existing.amount += amount;
        }
        else
        {
            foods.Add(new FoodAmount(food, amount));
        }
    }

    public void PlayAddFoodTween()
    {
        activeTween?.Kill();

        transform.localScale = initialScale;
        transform.localPosition = initialPosition;

        bagParticles.Play();
        
        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOPunchScale(
            Vector3.one * punchScale,
            tweenDuration,
            8,
            0.8f));

        seq.Join(transform.DOPunchPosition(
            UnityEngine.Random.insideUnitSphere * punchPosition,
            tweenDuration,
            10,
            0.9f));

        activeTween = seq;
    }

    public void OnMouseDown()
    {
        isMouseDown = true;
    }

    public void OnMouseUp()
    {
        isMouseDown = false;
    }
}
