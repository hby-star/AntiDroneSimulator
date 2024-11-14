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
        if (stats.gameObject.activeSelf)
            stats.OnHealthChanged += UpdateHealthUI;
        else
            gameObject.SetActive(false);
    }

    private void UpdateHealthUI()
    {
        slider.maxValue = stats.maxHeath;
        slider.value = stats.currentHeath;
    }
}