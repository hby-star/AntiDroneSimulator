using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

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

    // 持久化设置
    private string settingsFilePath = "AntiDroneSimulatorGameSettings.json";

    [Serializable]
    private class SettingsData
    {
        public float sensitivity;
        public float volume;
        public float playerHealth;
        public float bombRange;
        public float bombDamage;
        public float droneNum;
    }

    private SettingsData settingsData;

    void UpdateSensitivityText()
    {
        int sensitivity = (int)sensitivitySlider.value;
        sensitivityText.text = sensitivity.ToString();
    }

    void UpdateVolumeText()
    {
        int volume = (int)(volumeSlider.value * 10);
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

        LoadSettings();
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

    private void OnDestroy()
    {
        SaveSettings();
    }

    private void LoadSettings()
    {
        if (File.Exists(settingsFilePath))
        {
            string json = File.ReadAllText(settingsFilePath);
            settingsData = JsonUtility.FromJson<SettingsData>(json);

            sensitivitySlider.value = settingsData.sensitivity;
            volumeSlider.value = settingsData.volume;
            playerHeathSlider.value = settingsData.playerHealth;
            bombRangeSlider.value = settingsData.bombRange;
            bombDamageSlider.value = settingsData.bombDamage;
            droneNumSlider.value = settingsData.droneNum;
        }
        else
        {
            settingsData = new SettingsData
            {
                sensitivity = sensitivitySlider.value,
                volume = volumeSlider.value,
                playerHealth = playerHeathSlider.value,
                bombRange = bombRangeSlider.value,
                bombDamage = bombDamageSlider.value,
                droneNum = droneNumSlider.value
            };
        }
    }

    private void SaveSettings()
    {
        settingsData.sensitivity = sensitivitySlider.value;
        settingsData.volume = volumeSlider.value;
        settingsData.playerHealth = playerHeathSlider.value;
        settingsData.bombRange = bombRangeSlider.value;
        settingsData.bombDamage = bombDamageSlider.value;
        settingsData.droneNum = droneNumSlider.value;

        string json = JsonUtility.ToJson(settingsData);
        File.WriteAllText(settingsFilePath, json);
    }
}