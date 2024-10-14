using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
   public EntityStats stats;
   public Slider slider;

   private void Start()
   {
      stats = FindObjectOfType<Player>().GetComponent<EntityStats>();
      stats.OnHealthChanged += UpdateHealthUI;
   }

   private void UpdateHealthUI()
   {
      slider.maxValue = stats.maxHeath;
      slider.value = stats.currentHeath;
   }
}
