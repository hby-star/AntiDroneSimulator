using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
   private Entity entity;
   private EntityStats stats;
   private Transform myTransform;
   private Slider slider;

   private void Start()
   {
      entity = GetComponentInParent<Entity>();
      stats = GetComponentInParent<EntityStats>();
      myTransform = GetComponent<Transform>();
      slider = GetComponentInChildren<Slider>();

      stats.OnHealthChanged += UpdateHealthUI;
   }

   private void UpdateHealthUI()
   {
      slider.maxValue = stats.maxHeath;
      slider.value = stats.currentHeath;
   }
}
