using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
   private EntityStats stats;
   private Slider slider;

   private void Start()
   {
      stats = GetComponentInParent<EntityStats>();
      slider = GetComponentInChildren<Slider>();
      stats.OnHealthChanged += UpdateHealthUI;
   }

   private void UpdateHealthUI()
   {
      slider.maxValue = stats.maxHeath;
      slider.value = stats.currentHeath;
   }
}
