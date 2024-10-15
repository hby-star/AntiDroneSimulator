using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Settings : MonoBehaviour
{
    #region 玩家

    [Header("玩家")] public Slider playerHeathSlider;
    public TextMeshProUGUI playerHeathText;
    public Slider playerMoveSpeedSlider;
    public TextMeshProUGUI playerMoveSpeedText;
    public Slider normalBulletNumSlider;
    public TextMeshProUGUI normalBulletNumText;
    public Slider netBulletNumSlider;
    public TextMeshProUGUI netBulletNumText;
    public Slider empBulletNumSlider;
    public TextMeshProUGUI empBulletNumText;
    public Slider empBulletDurationSlider;
    public TextMeshProUGUI empBulletDurationText;

    void UpdatePlayerHeathText()
    {
        int playerHeath = (int)playerHeathSlider.value;
        playerHeathText.text = playerHeath.ToString();
    }

    void UpdatePlayerMoveSpeedText()
    {
        int playerMoveSpeed = (int)playerMoveSpeedSlider.value;
        playerMoveSpeedText.text = playerMoveSpeed.ToString();
    }

    void UpdateNormalBulletNumText()
    {
        int normalBulletNum = (int)normalBulletNumSlider.value;
        normalBulletNumText.text = normalBulletNum.ToString();
    }

    void UpdateNetBulletNumText()
    {
        int netBulletNum = (int)netBulletNumSlider.value;
        netBulletNumText.text = netBulletNum.ToString();
    }

    void UpdateEmpBulletNumText()
    {
        int empBulletNum = (int)empBulletNumSlider.value;
        empBulletNumText.text = empBulletNum.ToString();
    }

    void UpdateEmpBulletDurationText()
    {
        int empBulletDuration = (int)empBulletDurationSlider.value;
        empBulletDurationText.text = empBulletDuration.ToString();
    }

    #endregion

    #region 无人机集群

    [Header("无人机集群")] public Slider droneNumSlider;
    public TextMeshProUGUI droneNumText;
    public Slider detectDroneSpeedSlider;
    public TextMeshProUGUI detectDroneSpeedText;
    public Slider attackDroneSpeedSlider;
    public TextMeshProUGUI attackDroneSpeedText;
    public Slider bombRangeSlider;
    public TextMeshProUGUI bombRangeText;
    public Slider bombDamageSlider;
    public TextMeshProUGUI bombDamageText;

    void UpdateDroneNumText()
    {
        int droneNum = (int)droneNumSlider.value;
        droneNumText.text = droneNum.ToString();
    }

    void UpdateDetectDroneSpeedText()
    {
        int detectDroneSpeed = (int)detectDroneSpeedSlider.value;
        detectDroneSpeedText.text = detectDroneSpeed.ToString();
    }

    void UpdateAttackDroneSpeedText()
    {
        int attackDroneSpeed = (int)attackDroneSpeedSlider.value;
        attackDroneSpeedText.text = attackDroneSpeed.ToString();
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

    #endregion

    #region 系统

    [Header("系统")] public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityText;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;

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

    #endregion


    // 持久化设置
    private const string SETTINGS_FILE_PATH = "AntiDroneSimulatorGameSettings.json";

    [Serializable]
    private class SettingsData
    {
        // 玩家
        public float playerHealth;
        public float playerMoveSpeed;
        public float normalBulletNum;
        public float netBulletNum;
        public float empBulletNum;
        public float empBulletDuration;

        // 无人机集群
        public float droneNum;
        public float detectDroneSpeed;
        public float attackDroneSpeed;
        public float bombRange;
        public float bombDamage;

        // 系统
        public float sensitivity;
        public float volume;
    }

    private SettingsData settingsData;

    private void Awake()
    {
        // 玩家
        playerHeathSlider.onValueChanged.AddListener(delegate { UpdatePlayerHeathText(); });
        playerMoveSpeedSlider.onValueChanged.AddListener(delegate { UpdatePlayerMoveSpeedText(); });
        normalBulletNumSlider.onValueChanged.AddListener(delegate { UpdateNormalBulletNumText(); });
        netBulletNumSlider.onValueChanged.AddListener(delegate { UpdateNetBulletNumText(); });
        empBulletNumSlider.onValueChanged.AddListener(delegate { UpdateEmpBulletNumText(); });
        empBulletDurationSlider.onValueChanged.AddListener(delegate { UpdateEmpBulletDurationText(); });

        // 无人机集群
        droneNumSlider.onValueChanged.AddListener(delegate { UpdateDroneNumText(); });
        detectDroneSpeedSlider.onValueChanged.AddListener(delegate { UpdateDetectDroneSpeedText(); });
        attackDroneSpeedSlider.onValueChanged.AddListener(delegate { UpdateAttackDroneSpeedText(); });
        bombRangeSlider.onValueChanged.AddListener(delegate { UpdateBombRangeText(); });
        bombDamageSlider.onValueChanged.AddListener(delegate { UpdateBombDamageText(); });

        // 系统
        sensitivitySlider.onValueChanged.AddListener(delegate { UpdateSensitivityText(); });
        volumeSlider.onValueChanged.AddListener(delegate { UpdateVolumeText(); });

        LoadSettings();
    }

    private void Start()
    {
        // 玩家
        UpdatePlayerHeathText();
        UpdatePlayerMoveSpeedText();
        UpdateNormalBulletNumText();
        UpdateNetBulletNumText();
        UpdateEmpBulletNumText();
        UpdateEmpBulletDurationText();

        // 无人机集群
        UpdateDroneNumText();
        UpdateDetectDroneSpeedText();
        UpdateAttackDroneSpeedText();
        UpdateBombRangeText();
        UpdateBombDamageText();

        // 系统
        UpdateSensitivityText();
        UpdateVolumeText();
    }

    private void OnDestroy()
    {
        SaveSettings();
    }

    private void LoadSettings()
    {
        if (File.Exists(SETTINGS_FILE_PATH))
        {
            string json = File.ReadAllText(SETTINGS_FILE_PATH);
            settingsData = JsonUtility.FromJson<SettingsData>(json);

            // 玩家
            playerHeathSlider.value = settingsData.playerHealth;
            playerMoveSpeedSlider.value = settingsData.playerMoveSpeed;
            normalBulletNumSlider.value = settingsData.normalBulletNum;
            netBulletNumSlider.value = settingsData.netBulletNum;
            empBulletNumSlider.value = settingsData.empBulletNum;
            empBulletDurationSlider.value = settingsData.empBulletDuration;

            // 无人机集群
            droneNumSlider.value = settingsData.droneNum;
            detectDroneSpeedSlider.value = settingsData.detectDroneSpeed;
            attackDroneSpeedSlider.value = settingsData.attackDroneSpeed;
            bombRangeSlider.value = settingsData.bombRange;
            bombDamageSlider.value = settingsData.bombDamage;

            // 系统
            sensitivitySlider.value = settingsData.sensitivity;
            volumeSlider.value = settingsData.volume;
        }
        else
        {
            settingsData = new SettingsData
            {
                // 玩家
                playerHealth = playerHeathSlider.value,
                playerMoveSpeed = playerMoveSpeedSlider.value,
                normalBulletNum = normalBulletNumSlider.value,
                netBulletNum = netBulletNumSlider.value,
                empBulletNum = empBulletNumSlider.value,
                empBulletDuration = empBulletDurationSlider.value,

                // 无人机集群
                droneNum = droneNumSlider.value,
                detectDroneSpeed = detectDroneSpeedSlider.value,
                attackDroneSpeed = attackDroneSpeedSlider.value,
                bombRange = bombRangeSlider.value,
                bombDamage = bombDamageSlider.value,

                // 系统
                sensitivity = sensitivitySlider.value,
                volume = volumeSlider.value
            };
        }
    }

    private void SaveSettings()
    {
        // 玩家
        settingsData.playerHealth = playerHeathSlider.value;
        settingsData.playerMoveSpeed = playerMoveSpeedSlider.value;
        settingsData.normalBulletNum = normalBulletNumSlider.value;
        settingsData.netBulletNum = netBulletNumSlider.value;
        settingsData.empBulletNum = empBulletNumSlider.value;
        settingsData.empBulletDuration = empBulletDurationSlider.value;

        // 无人机集群
        settingsData.droneNum = droneNumSlider.value;
        settingsData.detectDroneSpeed = detectDroneSpeedSlider.value;
        settingsData.attackDroneSpeed = attackDroneSpeedSlider.value;
        settingsData.bombRange = bombRangeSlider.value;
        settingsData.bombDamage = bombDamageSlider.value;

        // 系统
        settingsData.sensitivity = sensitivitySlider.value;
        settingsData.volume = volumeSlider.value;

        string json = JsonUtility.ToJson(settingsData);
        File.WriteAllText(SETTINGS_FILE_PATH, json);
    }
}