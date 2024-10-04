using System;
using UnityEngine;

public class Gun : Equipment
{
    [Header("UI Info")] public Texture2D gunImage;
    public float imageWidth = 200f;
    public float imageHeight = 200f;
    public float imageEdge = 50f;
    public float bulletCountWidth = 210f;
    public float bulletCountHeight = 60f;
    public int bulletFontSize = 20;

    public Camera playerCamera;

    [Header("Audio Info")] [SerializeField]
    public AudioSource soundSource;

    [SerializeField] public AudioClip reloadSound;
    [SerializeField] public AudioClip fireSound;


    public enum GunType
    {
        HandGun,
        NetGun,
        EMPGun,
    }

    public int currentBullets;
    public int maxBullets;
    public GunType gunType;
    public bool isReloading;

    protected virtual void Start()
    {
        Type = EquipmentType.Gun;
    }

    public virtual void Fire()
    {
        currentBullets--;
        soundSource.PlayOneShot(fireSound);
    }

    public virtual bool CanFire()
    {
        return currentBullets > 0;
    }

    public virtual void ReloadStart()
    {
        soundSource.PlayOneShot(reloadSound);
        isReloading = true;
    }

    public virtual void ReloadEnd()
    {
        currentBullets = maxBullets;
        isReloading = false;
    }

    public virtual bool CanReload()
    {
        return currentBullets < maxBullets;
    }

    public void OnGUI()
    {
        if (InputManager.Instance.operateEntityNow &&
            InputManager.Instance.currentEntity is Player player &&
            UIManager.Instance.IsPopUpAllHidden())
        {
            Texture2D uiImage = null;

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

                uiImage = handGun.gunImage;
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

                uiImage = empGun.gunImage;
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

                uiImage = netGun.gunImage;
            }

            // Draw gun image
            if (uiImage)
            {
                GUI.DrawTexture(
                    new Rect(Screen.width - imageWidth - imageEdge, Screen.height - imageHeight - imageEdge / 2,
                        imageWidth, imageHeight), uiImage);
            }

            // Draw bullet count below gun image
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.fontSize = bulletFontSize;
            if (isReloading)
            {
                GUI.Label(
                    new Rect(Screen.width - bulletCountWidth,
                        Screen.height - bulletCountHeight, bulletCountWidth,
                        bulletCountHeight), "Reloading...", labelStyle);
            }
            else
            {
                GUI.Label(new Rect(Screen.width - bulletCountWidth,
                    Screen.height - bulletCountHeight, bulletCountWidth,
                    bulletCountHeight), "Bullet: " + currentBullets + " / " + maxBullets, labelStyle);
            }
        }
    }
}