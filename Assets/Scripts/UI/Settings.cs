using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityText;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;
    public Slider playerHeathSlider;
    public TextMeshProUGUI playerHeathText;
    public Slider bombRangeSlider;
    public TextMeshProUGUI bombRangeText;
    public Slider bombDamageSlider;
    public TextMeshProUGUI bombDamageText;
    public Slider droneNumSlider;
    public TextMeshProUGUI droneNumText;

    void UpdateSensitivityText()
    {
        int sensitivity = (int)sensitivitySlider.value;
        sensitivityText.text = sensitivity.ToString();
    }

    void UpdateVolumeText()
    {
        int volume = (int)(volumeSlider.value*10);
        volumeText.text = volume.ToString();
    }

    void UpdatePlayerHeathText()
    {
        int playerHeath = (int)playerHeathSlider.value;
        playerHeathText.text = playerHeath.ToString();
    }

    void UpdateBombRangeText()
    {
        int bombRange = (int)bombRangeSlider.value;
        bombRangeText.text = bombRange.ToString();
    }

    void UpdateBombDamageText()
    {
        int bombDamage = (int)bombDamageSlider.value;
        bombDamageText.text = bombDamage.ToString();
    }

    void UpdateDroneNumText()
    {
        int droneNum = (int)droneNumSlider.value;
        droneNumText.text = droneNum.ToString();
    }

    private void Awake()
    {
        sensitivitySlider.onValueChanged.AddListener(delegate { UpdateSensitivityText(); });
        volumeSlider.onValueChanged.AddListener(delegate { UpdateVolumeText(); });
        playerHeathSlider.onValueChanged.AddListener(delegate { UpdatePlayerHeathText(); });
        bombRangeSlider.onValueChanged.AddListener(delegate { UpdateBombRangeText(); });
        bombDamageSlider.onValueChanged.AddListener(delegate { UpdateBombDamageText(); });
        droneNumSlider.onValueChanged.AddListener(delegate { UpdateDroneNumText(); });
    }

    private void Start()
    {
        UpdateSensitivityText();
        UpdateVolumeText();
        UpdatePlayerHeathText();
        UpdateBombRangeText();
        UpdateBombDamageText();
        UpdateDroneNumText();
    }
}