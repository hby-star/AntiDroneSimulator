using System.Collections;
using UnityEngine;

public class NetGun : Gun
{
    public GameObject netPrefab; // Prefab for the net
    public float netSpeed = 10f;

    protected override void Start()
    {
        base.Start();

        gunType = GunType.NetGun;
    }

    public override void Fire()
    {
        //base.Fire();

        // Instantiate the net at the center of the screen
        Vector3 origin = playerCamera.transform.position + playerCamera.transform.forward;
        GameObject net = Instantiate(netPrefab, origin, Quaternion.identity);

        // Apply an initial forward force to the net
        Rigidbody rb = net.GetComponent<Rigidbody>();
        net.transform.forward = playerCamera.transform.forward;
        rb.velocity = playerCamera.transform.forward * netSpeed;
    }

    void OnGUI()
    {
        if (InputManager.Instance.operateEntityNow)
        {
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
                new Rect(centerX + squareSize / 2 - cornerLength, centerY - squareSize / 2, cornerLength, lineWidth),
                lineTexture);
            GUI.DrawTexture(
                new Rect(centerX + squareSize / 2 - lineWidth, centerY - squareSize / 2, lineWidth, cornerLength),
                lineTexture);

            // Bottom-left corner
            GUI.DrawTexture(
                new Rect(centerX - squareSize / 2, centerY + squareSize / 2 - lineWidth, cornerLength, lineWidth),
                lineTexture);
            GUI.DrawTexture(
                new Rect(centerX - squareSize / 2, centerY + squareSize / 2 - cornerLength, lineWidth, cornerLength),
                lineTexture);

            // Bottom-right corner
            GUI.DrawTexture(
                new Rect(centerX + squareSize / 2 - cornerLength, centerY + squareSize / 2 - lineWidth, cornerLength,
                    lineWidth), lineTexture);
            GUI.DrawTexture(
                new Rect(centerX + squareSize / 2 - lineWidth, centerY + squareSize / 2 - cornerLength, lineWidth,
                    cornerLength), lineTexture);

            // Horizontal line for crosshair
            GUI.DrawTexture(
                new Rect(centerX - crosshairLineLength / 2, centerY - lineWidth / 2, crosshairLineLength, lineWidth),
                lineTexture);
            // Vertical line for crosshair
            GUI.DrawTexture(
                new Rect(centerX - lineWidth / 2, centerY - crosshairLineLength / 2, lineWidth, crosshairLineLength),
                lineTexture);
        }
    }
}