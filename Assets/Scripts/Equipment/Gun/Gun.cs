using System;
using UnityEngine;

public class Gun : Equipment
{
    public Action onBulletCountChanged;

    [Header("UI Info")] public Sprite gunImage;

    [Header("Audio Info")] [SerializeField]
    public AudioSource soundSource;

    [SerializeField] public AudioClip reloadSound;
    [SerializeField] public AudioClip fireSound;

    public int currentBullets;
    public int maxBullets;
    public bool isReloading;

    void SettingsAwake()
    {
        if (UIManager.Instance)
            soundSource.volume *= UIManager.Instance.settingsPopUp.GetComponent<Settings>().volumeSlider.value;
    }

    protected virtual void Awake()
    {
        SettingsAwake();
    }

    protected virtual void Start()
    {
        Type = EquipmentType.Gun;
    }

    public virtual void Fire()
    {
        currentBullets--;
        soundSource.PlayOneShot(fireSound);

        onBulletCountChanged?.Invoke();
    }

    public bool CanFire()
    {
        return currentBullets > 0;
    }

    public void ReloadStart()
    {
        soundSource.PlayOneShot(reloadSound);
        isReloading = true;

        onBulletCountChanged?.Invoke();
    }

    public void ReloadEnd()
    {
        currentBullets = maxBullets;
        isReloading = false;

        onBulletCountChanged?.Invoke();
    }

    public bool CanReload()
    {
        return currentBullets < maxBullets;
    }

    public void OnGUI()
    {
        if (InputManager.Instance.operateEntityNow &&
            InputManager.Instance.currentEntity is Player player &&
            UIManager.Instance.IsPopUpAllHidden())
        {
            // Draw crosshair
            if (player.currentEquipment is HandGun handGun)
            {
                #region Hand Gun Crosshair

                int crosshairSize = 12;
                int lineLength = 6;
                int lineWidth = 2;
                float posX = Screen.width / 2 - crosshairSize / 4;
                float posY = Screen.height / 2 - crosshairSize / 2;

                // Create a 1x1 texture for lines
                Texture2D lineTexture = new Texture2D(1, 1);
                lineTexture.SetPixel(0, 0, Color.white);
                lineTexture.Apply();

                // Horizontal line
                GUI.DrawTexture(
                    new Rect(Screen.width / 2 - lineLength / 2, Screen.height / 2 - lineWidth / 2,
                        lineLength,
                        lineWidth), lineTexture);
                // Vertical line
                GUI.DrawTexture(
                    new Rect(Screen.width / 2 - lineWidth / 2, Screen.height / 2 - lineLength / 2,
                        lineWidth,
                        lineLength), lineTexture);
                // Left line
                GUI.DrawTexture(
                    new Rect(Screen.width / 2 - lineLength - crosshairSize / 2,
                        Screen.height / 2 - lineWidth / 2,
                        lineLength, lineWidth), lineTexture);
                // Right line
                GUI.DrawTexture(
                    new Rect(Screen.width / 2 + crosshairSize / 2, Screen.height / 2 - lineWidth / 2,
                        lineLength,
                        lineWidth), lineTexture);
                // Top line
                GUI.DrawTexture(
                    new Rect(Screen.width / 2 - lineWidth / 2,
                        Screen.height / 2 - lineLength - crosshairSize / 2,
                        lineWidth, lineLength), lineTexture);
                // Bottom line
                GUI.DrawTexture(
                    new Rect(Screen.width / 2 - lineWidth / 2, Screen.height / 2 + crosshairSize / 2,
                        lineWidth,
                        lineLength), lineTexture);

                #endregion
            }
            else if (player.currentEquipment is EMPGun empGun)
            {
                #region EMP Gun Crosshair

                int lineLength = 6;
                int lineWidth = 2;
                int circleRadius = 30;
                int circleSegments = 100;
                float centerX = Screen.width / 2;
                float centerY = Screen.height / 2;

                // Create a 1x1 texture for lines
                Texture2D lineTexture = new Texture2D(1, 1);
                lineTexture.SetPixel(0, 0, Color.white);
                lineTexture.Apply();

                // Horizontal line
                GUI.DrawTexture(new Rect(centerX - lineLength / 2, centerY - lineWidth / 2, lineLength, lineWidth),
                    lineTexture);
                // Vertical line
                GUI.DrawTexture(new Rect(centerX - lineWidth / 2, centerY - lineLength / 2, lineWidth, lineLength),
                    lineTexture);

                // Draw circular outline
                for (int i = 0; i < circleSegments; i++)
                {
                    float angle = i * Mathf.PI * 2 / circleSegments;
                    float x = Mathf.Cos(angle) * circleRadius;
                    float y = Mathf.Sin(angle) * circleRadius;

                    GUI.DrawTexture(
                        new Rect(centerX + x - lineWidth / 2, centerY + y - lineWidth / 2, lineWidth, lineWidth),
                        lineTexture);
                }

                #endregion
            }
            else if (player.currentEquipment is NetGun netGun)
            {
                #region Net Gun Crosshair

                int squareSize = 50;
                int cornerLength = 10;
                int lineWidth = 2;
                int crosshairLineLength = 6;
                float centerX = Screen.width / 2;
                float centerY = Screen.height / 2;

                // Create a 1x1 texture for lines
                Texture2D lineTexture = new Texture2D(1, 1);
                lineTexture.SetPixel(0, 0, Color.white);
                lineTexture.Apply();

                // Top-left corner
                GUI.DrawTexture(new Rect(centerX - squareSize / 2, centerY - squareSize / 2, cornerLength, lineWidth),
                    lineTexture);
                GUI.DrawTexture(new Rect(centerX - squareSize / 2, centerY - squareSize / 2, lineWidth, cornerLength),
                    lineTexture);

                // Top-right corner
                GUI.DrawTexture(
                    new Rect(centerX + squareSize / 2 - cornerLength, centerY - squareSize / 2, cornerLength,
                        lineWidth),
                    lineTexture);
                GUI.DrawTexture(
                    new Rect(centerX + squareSize / 2 - lineWidth, centerY - squareSize / 2, lineWidth, cornerLength),
                    lineTexture);

                // Bottom-left corner
                GUI.DrawTexture(
                    new Rect(centerX - squareSize / 2, centerY + squareSize / 2 - lineWidth, cornerLength, lineWidth),
                    lineTexture);
                GUI.DrawTexture(
                    new Rect(centerX - squareSize / 2, centerY + squareSize / 2 - cornerLength, lineWidth,
                        cornerLength),
                    lineTexture);

                // Bottom-right corner
                GUI.DrawTexture(
                    new Rect(centerX + squareSize / 2 - cornerLength, centerY + squareSize / 2 - lineWidth,
                        cornerLength,
                        lineWidth), lineTexture);
                GUI.DrawTexture(
                    new Rect(centerX + squareSize / 2 - lineWidth, centerY + squareSize / 2 - cornerLength, lineWidth,
                        cornerLength), lineTexture);

                // Horizontal line for crosshair
                GUI.DrawTexture(
                    new Rect(centerX - crosshairLineLength / 2, centerY - lineWidth / 2, crosshairLineLength,
                        lineWidth),
                    lineTexture);
                // Vertical line for crosshair
                GUI.DrawTexture(
                    new Rect(centerX - lineWidth / 2, centerY - crosshairLineLength / 2, lineWidth,
                        crosshairLineLength),
                    lineTexture);

                #endregion
            }
        }
    }
}