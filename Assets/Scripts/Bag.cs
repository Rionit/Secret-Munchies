using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Bag : MonoBehaviour
{
   public List<FoodScriptableObject> foods = new List<FoodScriptableObject>();
   public RectTransform progressBar;
   public float progressBarSpeed = 4f;

   private bool isMouseDown = false;
   private bool isPacked = false;
   
   private void Start()
   {
      GetComponent<Clickable>().onClick.AddListener(GameManager.Instance.OnOrderBagClick);
      progressBar.parent.gameObject.SetActive(false);
   }

   private void Update()
   {
      if (isPacked) return;
      
      if (foods.Count != 0)
         progressBar.parent.gameObject.SetActive(true);
      
      // TODO: Remove me
      progressBar.parent.gameObject.SetActive(true);
      
      float x = Mathf.Lerp(progressBar.sizeDelta.x, isMouseDown ? .2f : 0f, Time.deltaTime * progressBarSpeed);
      progressBar.sizeDelta = new Vector2(x, progressBar.sizeDelta.y);

      if (x >= 0.19f)
      {
         isPacked = GameManager.Instance.GrabItem(gameObject);
         progressBar.parent.gameObject.SetActive(!isPacked);
      }
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
